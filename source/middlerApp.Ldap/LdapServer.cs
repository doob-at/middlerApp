using System;
using System.DirectoryServices.Protocols;
using System.Net.Security;
using System.Security.Cryptography.X509Certificates;

namespace MiddlerApp.Ldap
{
    public class LdapServer
    {
        public bool Enabled { get; set; } = true;
        public string Server { get; set; }
        public int? Port { get; set; }

        public SecureConnectionType SecureConnectionType { get; set; } = SecureConnectionType.None;

        public bool TrustAllSslCertificates { get; set; }

        public LdapServer()
        {
            
        }

       
        internal LdapConnection Connect(LdapOptions ldapOptions)
        {

            var server =$"{Server ?? EnvironmentInfo.Instance.DomainName}:{Port ?? 389}";

            LdapConnection ldapConnection = new LdapConnection(server);



            switch (SecureConnectionType)
            {
                case SecureConnectionType.None:

                   
                    break;


                case SecureConnectionType.Ssl:
                    ldapConnection.SessionOptions.SecureSocketLayer = true;
                    if (TrustAllSslCertificates)
                    {
                        ldapConnection.SessionOptions.VerifyServerCertificate = (connection, certificate) => true;
                    }
                    break;

                //case SecureConnectionType.Tls:
                //    ldapConnection.Connect(server, port);
                //    ldapConnection.StartTransportLayerSecurity(true);
                //    break;

                default:
                    throw new ArgumentOutOfRangeException();
            }

            if (ldapOptions.Credential != null)
            {
                ldapConnection.AuthType = AuthType.Negotiate;
                ldapConnection.Bind(ldapOptions.Credential);
            }
            
            return ldapConnection;
        }

        
        private bool LdapConnectionOnUserDefinedServerCertValidationDelegate(object sender, X509Certificate certificate, X509Chain chain, SslPolicyErrors sslpolicyerrors)
        {
            return true;
        }

        
    }

    public class LdapServerBuilder
    {
        private LdapServer ldapServer { get; }

        public LdapServerBuilder()
        {
            ldapServer = new LdapServer();
        }

        public LdapServerBuilder UseServer(string server)
        {
            ldapServer.Server = server;
            return this;
        }

        public LdapServerBuilder UsePort(int port)
        {
            ldapServer.Port = port;
            return this;
        }

        public LdapServerBuilder UseSecureConnection(SecureConnectionType value, bool trustAllSslCertificates = false)
        {
            ldapServer.SecureConnectionType = value;
            ldapServer.TrustAllSslCertificates = trustAllSslCertificates;
            return this;
        }


        public static implicit operator LdapServer(LdapServerBuilder builder)
        {
            return builder.ldapServer;
        }
    }

}
