using iText.Kernel.Pdf.Canvas.Parser.Data;
using iText.Kernel.Pdf.Canvas.Parser.Listener;
using iText.Kernel.Pdf.Canvas.Parser; 

namespace HerkesYazarOlsun.Portal.Helpers
{
    public class PdfImageListener : IEventListener
    {
        public List<byte[]> Images { get; } = new();

        public void EventOccurred(IEventData data, EventType type)
        {
            if (type == EventType.RENDER_IMAGE)
            {
                var renderInfo = (ImageRenderInfo)data;
                var image = renderInfo.GetImage();
                if (image != null)
                {
                    Images.Add(image.GetImageBytes());
                }
            }
        }

        public ICollection<EventType> GetSupportedEvents()
        {
            return new HashSet<EventType> { EventType.RENDER_IMAGE };
        }
    }

}
