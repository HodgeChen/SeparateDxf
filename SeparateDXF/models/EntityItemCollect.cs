using netDxf.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SeparateDXF.models
{
    internal class EntityItemCollect
    {
        internal List<EntityItem> LinearcList { get; set; }
        internal List<EntityItem> CircleList { get; set; }
        internal List<EntityItem> PolylineList { get; set; }
        internal List<EntityItem> SplineList { get; set; }
        internal List<EntityItem> TextList { get; set; }
        internal List<EntityItem> MtextList { get; set; }

        public EntityItemCollect()
        {
            LinearcList = new List<EntityItem>();
            CircleList = new List<EntityItem>();
            PolylineList = new List<EntityItem>();
            SplineList = new List<EntityItem>();
            TextList = new List<EntityItem>();
            MtextList = new List<EntityItem>();
        }


        public void AttachEntity(EntityObject entity)
        {
            switch (entity.Type)
            {
                case EntityType.Line:
                case EntityType.Arc:
                    LinearcList.Add(new EntityItem(entity));
                    break;
                case EntityType.Circle:
                    CircleList.Add(new EntityItem(entity));
                    break;
                case EntityType.Polyline2D:
                    PolylineList.Add(new EntityItem(entity));
                    break;
                case EntityType.Spline:
                    SplineList.Add(new EntityItem(entity));
                    break;
                case EntityType.Text:
                    TextList.Add(new EntityItem(entity));
                    break;
                case EntityType.MText:
                    MtextList.Add(new EntityItem(entity));
                    break;
            }
        }
    }
}
