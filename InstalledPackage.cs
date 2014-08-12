using System;
using System.Collections.Generic;

namespace NuGetPackageAnalyzer
{
	public class InstalledPackage
	{
		public string Id { get; set; }
		public string Version { get; set; }
		public SortedSet<string> InstalledProjects { get; private set; }
		public string Projects
		{
			get { return string.Join(Environment.NewLine, InstalledProjects); }
		}

		public InstalledPackage(string packageId, string version)
		{
			Id = packageId;
			Version = version;

			InstalledProjects = new SortedSet<string>();
		}
	}
}
