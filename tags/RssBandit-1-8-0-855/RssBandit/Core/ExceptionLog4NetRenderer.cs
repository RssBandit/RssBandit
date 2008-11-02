using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using log4net.ObjectRenderer;
using log4net.Util;
using NewsComponents.Utils;

namespace RssBandit.Core
{
    class ExceptionLog4NetRenderer : IObjectRenderer
    {
        public void RenderObject(RendererMap rendererMap, object obj, System.IO.TextWriter writer)
        {
            var exception = obj as Exception;
            if (exception != null)
                writer.WriteLine(exception.ToDescriptiveString());
            else
                writer.Write(SystemInfo.NullText);
        }
    }
}
