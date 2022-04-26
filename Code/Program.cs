#nullable enable

using System;
using System.IO;
using Newtonsoft.Json;

public class Program {
	public static void Main(string[] args) {
		if (args.Length < 1) {
			Console.WriteLine("Please, provide a path to search in.");
			Console.WriteLine("For example:");
			Console.WriteLine("> dotnet run ./node_modules");
			return;
		}

		var path = args[0];
		var files = Directory.EnumerateFiles(path, "package.json", SearchOption.AllDirectories);
		var licenses = new HashSet<string>();

		foreach(var file in files) {
			var fileContent = File.ReadAllText(file);

			IPackage? package;

			try {
				package = JsonConvert.DeserializeObject<Package>(fileContent);
			} catch {
				try {
					package = JsonConvert.DeserializeObject<PackageFull>(fileContent);
				} catch {
					try {
						package = JsonConvert.DeserializeObject<PackageList>(fileContent);
					} catch {
						continue;
					}
				}
			}

			if (package is null
				|| package.Licensing is null
				|| package.Licensing.Equals(""))
			{
				continue;
			}

			licenses.Add(package.Licensing);
		}

		foreach(var license in licenses) {
			Console.WriteLine(license);
		}
	}
}

interface IPackage {
	public string Licensing { get; }
}

public class FullLicense {
	public string Type { get; set; }
	public string Url { get; set; }

	public FullLicense(string type, string url) {
		Type = type;
		Url = url;
	}
}

public class PackageList: IPackage {
	private FullLicense[] Licenses { get; set; }

	string IPackage.Licensing {
		get {
			var accumulator = "";

			foreach(var license in Licenses) {
				accumulator += license.Type;
			}

			return accumulator;
		}
	}

	public PackageList(FullLicense[] licenses) {
		Licenses = licenses;
	}
}

public class PackageFull: IPackage {
	private FullLicense License { get; set; }

	string IPackage.Licensing => License.Type;

    public PackageFull(FullLicense license) {
		License = license;
	}
}

public class Package: IPackage {
	private string License { get; set; }

	string IPackage.Licensing => License;

	public Package(string license) {
		License = license;
	}
}
