using System.Runtime.Serialization;

using NetTopologySuite.Geometries;

namespace NetTopologySuite.Features
{
    internal static class InternalExtensions
    {
        public static Envelope GetBoundingBox(this SerializationInfo info)
        {
            return info.GetBoolean("has_bbox")
                ? new Envelope(info.GetDouble("bbox_x1"), info.GetDouble("bbox_x2"), info.GetDouble("bbox_y1"), info.GetDouble("bbox_y2"))
                : null;
        }

        public static void AddBoundingBox(this SerializationInfo info, Envelope boundingBox)
        {
            if (boundingBox is null)
            {
                info.AddValue("has_bbox", false);
            }
            else
            {
                info.AddValue("has_bbox", true);
                info.AddValue("bbox_x1", boundingBox.MinX);
                info.AddValue("bbox_x2", boundingBox.MaxX);
                info.AddValue("bbox_y1", boundingBox.MinY);
                info.AddValue("bbox_y2", boundingBox.MaxY);
            }
        }
    }
}
