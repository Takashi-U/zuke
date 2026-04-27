using System.Net;
using System.Text;
namespace Zuke.Core.Diff; public sealed class HtmlDiffRenderer { public string Render(string oldName,string newName,DiffResult r){ var sb=new StringBuilder(); sb.Append("<html><body><h1>diff</h1><pre>"); sb.Append(WebUtility.HtmlEncode(r.UnifiedText)); sb.Append("</pre></body></html>"); return sb.ToString(); } }
