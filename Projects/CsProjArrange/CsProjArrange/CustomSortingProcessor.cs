using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CsProjArrange
{
	public class CustomSortingProcessor
	{
		private string _rootWorkspaceDir;
		private string[] _useExtensions = new[]
		{
			"*.csproj",
			"*.shproj",
			"*.projitems",
		};

		public void ProcessSorting(string rootSearchDir, bool xamarinOnly, bool checkoutedOnly, bool usePredefinedFolders,
			Action<string, string, CsProjArrange.ArrangeOptions> runSorting)
		{
			//Console.WriteLine(rootSearchDir);
			//Console.WriteLine(searchRecursive);
			//Console.WriteLine(updateRecursive);
			//Console.WriteLine(usePredefinedFolders);
			//return;

			string[] searchFolders;
			if (usePredefinedFolders)
			{
				if (rootSearchDir.StartsWith(@"D:\Projects\"))
				{
					searchFolders = new[]
					{
						@"D:\Projects\Lara\Lara-Xamarin",
						@"D:\Projects\Lara_Sync\Lara_Sync-Xamarin",
						@"D:\Projects\Nestle_Impuls_limiGo\Nestle_Impuls_limiGo",
						@"D:\Projects\SFA\SFA-Xamarin",
					};
				}
				else if (rootSearchDir.StartsWith(@"D:\ProjectsPartners\"))
				{
					searchFolders = new[]
					{
						@"D:\ProjectsPartners\AuthenticationService",
						@"D:\ProjectsPartners\PaymentService",
						@"D:\ProjectsPartners\AccountingService",
					};
				}
				else
					throw new ArgumentOutOfRangeException();
			}
			else
				searchFolders = new[] { rootSearchDir };

			foreach (string searchFolder in searchFolders)
				this.ProcessSorting(searchFolder, xamarinOnly, checkoutedOnly, runSorting);
		}

		public void ProcessSorting(string rootSearchDir, bool xamarinOnly, bool checkoutedOnly,
			Action<string, string, CsProjArrange.ArrangeOptions> runSorting)
		{
			string path = Path.GetDirectoryName(rootSearchDir);
			_rootWorkspaceDir = path;
			if (path.IndexOf(@"\", 3) >= 0)
				_rootWorkspaceDir = path.Substring(0, path.IndexOf(@"\", 3));

			var filesToProcess = new List<string>();
			foreach (var fileExt in _useExtensions)
				filesToProcess.AddRange(Directory.GetFiles(rootSearchDir, fileExt, SearchOption.AllDirectories));

			string tempFilePath = Path.GetTempFileName();

			foreach (var filePath in filesToProcess)
			{
				if (xamarinOnly)
				{
					if (filePath.Contains(".Client") == false
						&& filePath.Contains(".Android") == false
						&& filePath.Contains(".iOS") == false
						&& filePath.Contains(".UWP") == false)
						continue;
				}

				bool isReadOnlyFile = File.GetAttributes(filePath).HasFlag(FileAttributes.ReadOnly);
				if (checkoutedOnly && isReadOnlyFile)
					continue;

				runSorting(filePath, tempFilePath, CsProjArrange.ArrangeOptions.CombineRootElements | CsProjArrange.ArrangeOptions.SortRootElements);

				byte[] tempFileContent = File.ReadAllBytes(tempFilePath);
				if (File.ReadAllBytes(filePath).SequenceEqual(tempFileContent))
					continue;

				if (isReadOnlyFile)
				{
					string tfsFilePath = filePath.Replace(_rootWorkspaceDir, "$").Replace(@"\", "/");
					var startInfo = new ProcessStartInfo(@"c:\Program Files (x86)\Microsoft Visual Studio 12.0\Common7\IDE\TF.exe", $@" checkout ""{tfsFilePath}""");
					startInfo.WorkingDirectory = _rootWorkspaceDir;
					var process = Process.Start(startInfo);
					process.WaitForExit();
				}

				File.WriteAllBytes(filePath, tempFileContent);
			}
		}
	}
}
