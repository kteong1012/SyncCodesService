// See https://aka.ms/new-console-template for more information
using System.Xml.Linq;

Console.WriteLine("Start");

public class AdjustTool
{
    public static void Adjust(string workPlace, string csprojPath, string srcDir)
    {
        DirectoryInfo dir = new DirectoryInfo(srcDir);
        if (!dir.Exists)
        {
            return;
        }
        srcDir = srcDir.Replace("\\", "/");
        XDocument doc = XDocument.Load(csprojPath);
        XElement project = doc.Elements().First(e => e.Name.LocalName == "Project");
        var comments = doc.Nodes().Where(n => n.NodeType == System.Xml.XmlNodeType.Comment).ToList();
        foreach (var c in comments)
        {
            c.Remove();
        }
        var itemGroups = project.Elements().Where(e => e.Name.LocalName == "ItemGroup");
        List<XElement> delCompile = new List<XElement>();
        foreach (XElement itemGroup in itemGroups)
        {
            foreach (var element in itemGroup.Elements())
            {
                if (element.Name.LocalName == "Compile")
                {
                    delCompile.Add(element);
                }
            }
        }
        foreach (XElement item in delCompile)
        {
            item.Remove();
        }
        var infos = GetAllFiles(dir, "*.cs");
        foreach (var csFile in infos)
        {
            string fullPath = csFile.FullName.Replace("\\", "/");
            string localPath = fullPath.Substring(workPlace.Length + 1, fullPath.Length - workPlace.Length -1).Replace("/","\\");
            XElement firstItemGroup = project.Elements().First(e => e.Name.LocalName == "ItemGroup");
            XElement compile = new XElement(project.Name.ToString().Replace("Project", "Compile"), new XAttribute("Include", localPath));
            firstItemGroup.Add(compile);
        }
        doc.Save(csprojPath);
        Console.WriteLine($"刷新成功: {Path.GetFileNameWithoutExtension(csprojPath)}");
    }

    private static List<FileInfo> GetAllFiles(DirectoryInfo dir, string pattern)
    {
        List<FileInfo> infos = new List<FileInfo>();
        foreach (var file in dir.GetFiles(pattern))
        {
            infos.Add(file);
        }
        foreach (var subDir in dir.GetDirectories())
        {
            foreach (var file in GetAllFiles(subDir, pattern))
            {
                infos.Add(file);
            }
        }
        return infos;
    }
}