/*

Class: FirebaseRoot.cs
==============================================
Last update: 2016-03-14  (by Dikra)
==============================================

Copyright (c) 2016  M Dikra Prasetya

 * MIT LICENSE
 *
 * Permission is hereby granted, free of charge, to any person obtaining
 * a copy of this software and associated documentation files (the
 * "Software"), to deal in the Software without restriction, including
 * without limitation the rights to use, copy, modify, merge, publish,
 * distribute, sublicense, and/or sell copies of the Software, and to
 * permit persons to whom the Software is furnished to do so, subject to
 * the following conditions:
 *
 * The above copyright notice and this permission notice shall be
 * included in all copies or substantial portions of the Software.
 *
 * THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND,
 * EXPRESS OR IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF
 * MERCHANTABILITY, FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT.
 * IN NO EVENT SHALL THE AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY
 * CLAIM, DAMAGES OR OTHER LIABILITY, WHETHER IN AN ACTION OF CONTRACT,
 * TORT OR OTHERWISE, ARISING FROM, OUT OF OR IN CONNECTION WITH THE
 * SOFTWARE OR THE USE OR OTHER DEALINGS IN THE SOFTWARE.

*/

using System.Security.Cryptography.X509Certificates;
using System.Net.Security;
using System.Net;

namespace FirebaseCSharp
{
    internal class FirebaseRoot : Firebase
    {
        private static bool firstTimeInitiated = true;
        private string host;
        private string cred;

        /// <summary>
        /// Returns .json endpoint to this Firebase point
        /// </summary>
        public override string Endpoint
        {
            get
            {
                return "https://" + root.Host + "/.json";
            }
        }

        /// <summary>
        /// Credential for auth parameter
        /// </summary>
        public override string Credential
        {
            get
            {
                return cred;
            }

            set
            {
                cred = value;
            }
        }

        public FirebaseRoot(string _host, string _cred = "")
        {
            if (firstTimeInitiated)
            {
                ServicePointManager.ServerCertificateValidationCallback += new RemoteCertificateValidationCallback((sender, certificate, chain, policyErrors) => { return true; });
                firstTimeInitiated = false;
            }

            root = this;
            host = _host;
            cred = _cred;
        }


        /// <summary>
        /// Returns main host of Firebase
        /// </summary>
        public override string Host
        {
            get
            {
                return host;
            }
        }

        private static bool RemoteCertificateValidationCallback(object sender, X509Certificate certificate, X509Chain chain, SslPolicyErrors sslPolicyErrors)
        {
            return true; // override certificate, we trust Firebase :D
        }
    }
}
