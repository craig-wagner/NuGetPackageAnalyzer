using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Xml.Linq;

namespace NuGetPackageAnalyzer
{
	public class PackageAnalyzer
	{
		private string _solutionFilePath;
		private IDictionary<string, InstalledPackage> _installedPackages;

		public PackageAnalyzer(string solutionFilePath)
		{
			_solutionFilePath = solutionFilePath;
			_installedPackages = new Dictionary<string, InstalledPackage>();
		}

		public ICollection<InstalledPackage> Analyze()
		{
			string slnFolder = Path.GetDirectoryName(_solutionFilePath);

			string slnContents;
			using (StreamReader reader = new StreamReader(_solutionFilePath))
			{
				slnContents = reader.ReadToEnd();
			}

			var slnLines = slnContents.Split(Environment.NewLine.ToCharArray(), StringSplitOptions.RemoveEmptyEntries);
			var projects = slnLines.Where(l => l.Contains("FAE04EC0-301F-11D3-BF4B-00C04F79EFBC")).ToList();

			foreach (var project in projects)
			{
				var values = project.Substring(project.IndexOf('=') + 1).Replace("\"", String.Empty);
				var parts = values.Split(',');
				var projectFolder = Path.GetDirectoryName(Path.GetFullPath(Path.Combine(slnFolder, parts[1].Trim())));

				var packagesConfigPath = Path.Combine(projectFolder, "packages.config");
				if (File.Exists(packagesConfigPath))
				{
					ProcessPackagesConfig(parts[0].Trim(), packagesConfigPath);
				}
			}

			var installedPackages = _installedPackages.Values.ToList();
			installedPackages.Sort(new InstalledPackageComparer());

			return installedPackages;
		}

		private void ProcessPackagesConfig(string projectName, string packagesConfigPath)
		{
			var packagesConfig = XDocument.Load(packagesConfigPath);
			var packages = (from p in packagesConfig.Descendants("package")
							select new
							{
								Id = p.Attribute("id").Value,
								Version = p.Attribute("version").Value
							}).ToList();

			foreach (var package in packages)
			{
				var key = package.Id + package.Version;

				if (!_installedPackages.ContainsKey(key))
				{
					_installedPackages[key] = new InstalledPackage(package.Id, package.Version);
				}

				var packageInfo = _installedPackages[key];
				packageInfo.InstalledProjects.Add(projectName);
			}
		}

		private class InstalledPackageComparer : IComparer<InstalledPackage>
		{
			public int Compare(InstalledPackage x, InstalledPackage y)
			{
				int returnValue = 0;

				if (x.Id == y.Id)
				{
					var xVersion = new Version(x.Version);
					var yVersion = new Version(y.Version);

					if (xVersion < yVersion)
					{
						returnValue = -1;
					}
					else
					{
						returnValue = 1;
					}
				}
				else
				{
					returnValue = String.Compare(x.Id, y.Id);
				}

				return returnValue;
			}
		}
	}
}
