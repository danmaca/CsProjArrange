using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Linq;

namespace CsProjArrange
{
	public class InputDataModificator
	{
		public void ModifyInputData(XDocument input)
		{
			var projRootElem = (XElement)input.Nodes().First();
			foreach (var groupNode in projRootElem.Nodes().OfType<XElement>().Where(obj => obj.Name.LocalName == "ItemGroup"))
			{
				foreach (XElement packageNode in groupNode.Nodes().OfType<XElement>().Where(obj => obj.Name.LocalName == "PackageReference"))
				{
					var versionNode = packageNode.Nodes().OfType<XElement>().FirstOrDefault(obj => obj.Name.LocalName == "Version");
					if (versionNode != null)
					{
						packageNode.SetAttributeValue("Version", versionNode.Value);
						versionNode.Remove();
					}
				}
			}
		}
	}
}