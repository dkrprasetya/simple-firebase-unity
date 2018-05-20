/*

Class: FirebaseRoot.cs
==============================================
Last update: 2018-05-20  (by Dikra)
==============================================


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
using System.Collections;

namespace SimpleFirebaseUnity
{
    internal class FirebaseRoot : Firebase
    {
        protected static bool firstTimeInitiated = true;
        protected string host;
        protected string cred;

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

        /// <summary>
        /// Returns .json endpoint to Firebase Rules.
        /// </summary>
        public override string RulesEndpoint
        {
            get 
            {
                return "https://" + root.Host + "/.settings/rules.json";
            }
        }

        /// <summary>
        /// Copy this instance.
        /// </summary>
        public FirebaseRoot Copy()
        {
            return new FirebaseRoot (host, cred);
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="SimpleFirebaseUnity.FirebaseRoot"/> class.
        /// </summary>
        /// <param name="_host">Host.</param>
        /// <param name="_cred">Cred.</param>
        public FirebaseRoot(string _host, string _cred = "")
        {
            if (firstTimeInitiated)
            {
                ServicePointManager.ServerCertificateValidationCallback += RemoteCertificateValidationCallback;
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

        /// <summary>
        /// Starts the coroutine.
        /// </summary>
        /// <param name="routine">Routine.</param>
        public void StartCoroutine(IEnumerator routine)
        {
            FirebaseManager.Instance.StartCoroutine (routine);
        }

        /// <summary>
        /// Stops the coroutine.
        /// </summary>
        /// <param name="routine">Routine.</param>
        public void StopCoroutine(IEnumerator routine)
        {
            FirebaseManager.Instance.StopCoroutine (routine);
        }
    }
}
