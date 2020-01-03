using System;
using System.Collections.Generic;
using System.Linq;
using System.Xml;
using System.Xml.Linq;

namespace CsProjArrange
{
	/// <summary>
	/// Orders the nodes by nearest element name with some element names optionally stuck to the top in the supplied order.
	/// </summary>
	public class NodeNameComparer : IComparer<XNode>
	{
		public NodeNameComparer(IList<string> stickyElementNames = null, CsProjArrange.ArrangeOptions options = CsProjArrange.ArrangeOptions.None)
		{
			StickyElementNames = stickyElementNames ?? new string[] { };
		}

		public IList<string> StickyElementNames
		{
			get;
			set;
		}

		public int Compare(XNode x, XNode y)
		{
			if (x == null) throw new ArgumentNullException("x");
			if (y == null) throw new ArgumentNullException("y");
			string xName = GetName(x);
			string yName = GetName(y);
			var stickyElement1 = StickyElementNames.IndexOf(xName);
			var stickyElement2 = StickyElementNames.IndexOf(yName);
			if ((stickyElement1 == -1) && (stickyElement2 == -1))
			{
				return String.Compare(xName, yName, StringComparison.InvariantCulture);
			}
			return Compare(stickyElement1, stickyElement2);
		}

		private static int Compare(int stickyElement1, int stickyElement2)
		{
			if ((stickyElement2 == -1) || ((stickyElement1 != -1) && (stickyElement1 < stickyElement2)))
			{
				return -1;
			}
			if ((stickyElement1 == -1) || ((stickyElement2 != -1) && (stickyElement1 > stickyElement2)))
			{
				return 1;
			}

			return 0;
		}

		private string GetName(XNode node)
		{
			string name = null;
			if (node.NodeType == XmlNodeType.Element)
			{
				var element = (XElement)node;
				name = element.Name.LocalName;

				name += this.TryAddSubNodeSuffix(element, "VSToolsPath");
				name += this.TryAddSubNodeSuffix(element, "PostBuildEvent");
				name += this.TryAddSubNodeSuffix(element, "Folder");
				name += this.TryAddAttribSuffix(element, "Import", "Project", @"$(MSBuildExtensionsPath)\$(MSBuildToolsVersion)\Microsoft.Common.props");
			}

			return name;
		}

		private string TryAddSubNodeSuffix(XElement element, string subNodeName)
		{
			if (element.Nodes().OfType<XElement>().Any(obj => obj.Name.LocalName == subNodeName))
				return "_" + subNodeName;
			return string.Empty;
		}

		private string TryAddAttribSuffix(XElement element, string elemName, string attribName, string attribValue)
		 {
			if (element.Name.LocalName == elemName
				&& element.Attributes().Any(obj => obj.Name.LocalName == attribName && obj.Value == attribValue))
				return ":" + attribName + "=" + attribValue;
			return string.Empty;
		}
	}
}