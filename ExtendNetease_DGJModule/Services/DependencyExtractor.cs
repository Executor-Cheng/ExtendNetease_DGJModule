using System;
using System.IO;
using System.Reflection;

namespace ExtendNetease_DGJModule.Services
{
    public sealed class DependencyExtractor : IDisposable
    {
        private readonly string _dependencyDirectory;

        public DependencyExtractor()
        {
            _dependencyDirectory = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.Personal), @"弹幕姬\plugins\Assembly");
            AppDomain.CurrentDomain.AssemblyResolve += CurrentDomain_AssemblyResolve;
        }

        public void Extract()
        {
            if (!Directory.Exists(_dependencyDirectory))
            {
                Directory.CreateDirectory(_dependencyDirectory);
            }
            string filePath = Path.Combine(_dependencyDirectory, "BouncyCastle.Crypto.dll");
            if (!File.Exists(filePath))
            {
                File.WriteAllBytes(filePath, Properties.Resources.BouncyCastle_Crypto);
            }
            filePath = Path.Combine(_dependencyDirectory, "QRCoder.dll");
            if (!File.Exists(filePath))
            {
                File.WriteAllBytes(filePath, Properties.Resources.QRCoder);
            }
        }

        private Assembly CurrentDomain_AssemblyResolve(object sender, ResolveEventArgs args)
        {
            int ix = args.Name.IndexOf(',');
            string dllName = ix == -1 ? args.Name : args.Name.Substring(0, ix);
            if (dllName == "BouncyCastle.Crypto")
            {
                return Assembly.LoadFrom(Path.Combine(_dependencyDirectory, "BouncyCastle.Crypto.dll"));
            }
            if (dllName == "QRCoder")
            {
                return Assembly.LoadFrom(Path.Combine(_dependencyDirectory, "QRCoder.dll"));
            }
            return null;
        }

        public void Dispose()
        {
            AppDomain.CurrentDomain.AssemblyResolve -= CurrentDomain_AssemblyResolve;
        }
    }
}
