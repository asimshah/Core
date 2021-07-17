using Fastnet.Core;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Authorization;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Fastnet.Blazor.Core
{
    public enum TokenRefreshMode
    {
        None,
        Periodic,
        BeforeExpiry
    }
    public partial class TokenManagement : IDisposable
    {
        [Parameter] public string NavigateLinkOnExpiry { get; set; } = "/";
        [Parameter] public TokenRefreshMode Mode { get; set; }
        /// <summary>
        /// Interval in minutes for periodic refresh or interval before expiry according to Mode
        /// </summary>
        [Parameter] public int Interval { get; set; } = 5;// in minutes, period interval or interval before expiry depending upon Mode
        [Inject] private IServiceProvider serviceProvider { get; set; }
        [Inject] private NavigationManager navigationManager { get; set; }
        private ILogger log;
        private LocalStorage localStorage;
        private CancellationTokenSource cts;
        private Task backgroundTokenMonitorTask;
        private SimpleAuthenticationStateProvider authenticationStateProvider;
        private IAuthenticationService authenticationService;

        ///<inheritdoc/>
        protected override async Task OnInitializedAsync()
        {
            log = serviceProvider.GetService(typeof(ILogger<TokenManagement>)) as ILogger;
            log.Information($"{nameof(TokenManagement)} OnInitialized()");
            localStorage = serviceProvider.GetService(typeof(LocalStorage)) as LocalStorage;
            authenticationService = serviceProvider.GetService(typeof(IAuthenticationService)) as IAuthenticationService;
            if (localStorage == null || authenticationService == null)
            {
                log.Error($"required services not found - have you called AddSimpleAuthentication()?");
            }
            authenticationStateProvider = serviceProvider.GetService(typeof(AuthenticationStateProvider)) as SimpleAuthenticationStateProvider;
            if (authenticationStateProvider != null)
            {
                authenticationStateProvider.AuthenticationStateChanged += AuthenticationStateProvider_AuthenticationStateChanged;
            }
            if(await HasTokenExpiredAsync())
            {
                await ChangeStateToAnonymous();
            }
        }

        private async void AuthenticationStateProvider_AuthenticationStateChanged(Task<AuthenticationState> task)
        {
            var user = (await task).User;
            if(user.Claims == null || user.Claims.Count() == 0)
            {
                // no user - has logged out
                StopMonitor();
            }
            else
            {
                await RestartMonitorAsync();
            }
        }
        ///<inheritdoc/>
        protected override async Task OnParametersSetAsync()
        {

            if (await IsLoggedInAsync())
            {
                await RestartMonitorAsync();
            }

        }
        public async Task RestartMonitorAsync()
        {

            if (Mode != TokenRefreshMode.None)
            {
                await StartBackgroundTokenMonitorAsync();
            }
            else
            {
                await LogTokenExpiredAsync();
            }
        }
        public void StopMonitor()
        {
            if (cts != null)
            {
                cts.Cancel();
                backgroundTokenMonitorTask.Dispose();
                cts = null;
                backgroundTokenMonitorTask = null;
                log.Information("Background token monitor stopped");
            }
        }

        ///<inheritdoc/>
        public void Dispose()
        {
            StopMonitor();
            log.Information($"{nameof(TokenManagement)} Dispose()");
        }
        private async Task<bool> IsLoggedInAsync()
        {
            var user = (await authenticationStateProvider.GetAuthenticationStateAsync()).User;
            if (user.Claims == null || user.Claims.Count() == 0)
            {
                return false;
            }
            return true;
        }
        private async Task StartBackgroundTokenMonitorAsync()
        {
            if (cts == null)
            {
                log.Information($"Background token monitor started, mode is {Mode.ToString()}, interval is {Interval} minutes");
                await LogTokenExpiredAsync();
                cts = new CancellationTokenSource();
                backgroundTokenMonitorTask = Task.Run(async () => {
                    while (!cts.IsCancellationRequested)
                    {
                        var interval = Mode == TokenRefreshMode.Periodic ? Interval * 60 * 1000 : 30 * 1000;// check expiry every 30 seconds 
                        await Task.Delay(interval, cts.Token);
                        if(await HasTokenExpiredAsync())
                        {
                            log.Warning($"Token has expired unexpectedly! Is the Interval too long relative to the expiry time?");
                            await ChangeStateToAnonymous();
                            navigationManager.NavigateTo(NavigateLinkOnExpiry);
                        }
                        switch(Mode)
                        {
                            case TokenRefreshMode.Periodic:
                                await RefreshTokenAsync();
                                break;
                            case TokenRefreshMode.BeforeExpiry:
                                if(await DoesTokenNeedRefreshing())
                                {
                                    await RefreshTokenAsync();
                                }
                                break;
                        }
                    }
                }, cts.Token);
            }
        }
        private async Task RefreshTokenAsync()
        {
            var result = await authenticationService.RefreshTokenAsync();
            if (!result.Success)
            {
                log.Error($"RefreshTokenAsync() failed");
                await ChangeStateToAnonymous();
            }
            await LogTokenExpiredAsync();
        }

        private async Task ChangeStateToAnonymous()
        {
            if (authenticationService is AuthenticationServiceBase)
            {
                var service = authenticationService as AuthenticationServiceBase;
                await service.ClearCurrentTokenAsync();
            }
            await authenticationStateProvider.NotifyUserAuthenticationAsync();
            navigationManager.NavigateTo(NavigateLinkOnExpiry);
        }

        private async Task LogTokenExpiredAsync()
        {
            var expiryDateTime = await GetTokenExpiryDateTimeAsync();
            if (expiryDateTime.HasValue)
            {
                var now = DateTimeOffset.Now;
                if (expiryDateTime < now)
                {
                    log.Warning($"token expired at {expiryDateTime.Value.ToDefaultWithTime()}");
                }
                else
                {
                    log.Information($"{now.ToDefaultWithTime()}: token expiry date/time is {expiryDateTime.Value.ToDefaultWithTime()}, in {(expiryDateTime.Value - now).TotalMinutes.ToString("#.00")} minutes");
                }
            }
            else
            {
                log.Information($"no token found");
            }
        }
        private async Task<DateTimeOffset?> GetTokenExpiryDateTimeAsync()
        {
            var token = await localStorage.GetAsync<string>("authToken");
            if (string.IsNullOrWhiteSpace(token))
            {
                return null;
            }
            return Fastnet.Blazor.Core.JwtParser.GetTokenExpiryDateTime(token);
        }
        private async Task<bool> DoesTokenNeedRefreshing()
        {
            var expiryDateTime = await GetTokenExpiryDateTimeAsync();
            if (expiryDateTime != null && expiryDateTime.HasValue)
            {
                var remaining = expiryDateTime - DateTimeOffset.Now;
                return remaining.Value.TotalMinutes < Interval;
            }
            return true;
        }
        private async Task<bool> HasTokenExpiredAsync()
        {
            var expiryDateTime = await GetTokenExpiryDateTimeAsync();
            if(expiryDateTime != null && expiryDateTime.HasValue)
            {
                return expiryDateTime.Value < DateTimeOffset.Now;
            }
            return true;
        }
    }
}
