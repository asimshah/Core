namespace Fastnet.Core
{
    /// <summary>
    /// 
    /// </summary>
    public class MailOptions
    {
        /*
         * some mail settings:
         *  1.  SmtpServer: smtp.gmail.com
         *      Username: asimshah2009@gmail.com
         *      Password: Transcend-b0se
         *      Port: 587
         *      EnableSSL: true
         *      
         *  2.  SmtpServer: smtp.gmail.com
         *      Username: mbatbox@gmail.com
         *      Password: mD5FJ3d49JaA
         *      Port: 587
         *      EnableSSL: true
         *      
         *  3.  SmtpServer: 127.0.0.1
         *      Username: null
         *      Password: null
         *      Port: 25
         *      EnableSSL: false
         */
        /// <summary>
        /// 
        /// </summary>
        public string SmtpServer { get; set; } = "smtp.gmail.com";
        /// <summary>
        /// 
        /// </summary>
        public string Username { get; set; } = "asimshah2009@gmail.com";
        /// <summary>
        /// 
        /// </summary>
        public string Password { get; set; } = "Transcend-b0se";
        /// <summary>
        /// 
        /// </summary>
        public int Port { get; set; } = 587;
        /// <summary>
        /// 
        /// </summary>
        public bool EnableSsl { get; set; } = true;
    }
}
