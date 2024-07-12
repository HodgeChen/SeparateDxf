using netDxf;
using netDxf.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Controls;

namespace SeparateDXF.models
{
    internal class LoopItem
    {
        public List<EntityObject> loopEntityList { get; set; }
        private RectBox rectBox { get; set; }
        private List<Vector3> verticeList { get; set; }
        private bool messFlag;

        public LoopItem()
        {
            loopEntityList = new List<EntityObject>();
            rectBox = new RectBox();
            verticeList = new List<Vector3>();
            messFlag = false;
        }

        //attachtype
        //0 = joint to end of the loop
        //1 = reverse the entity and joint to end of the loop
        //2 = joint to head of the loop
        //3 = reverse the entity and joint to head of the loop
        //4 = messed loop item
        public void AttachGeomItem(EntityObject entity, List<Vector3> vertices, RectBox rectbox, int attachtype = 0)
        {
            loopEntityList.Add(entity);
            if (vertices.Count > 0)
            {
                if (verticeList.Count == 0) { verticeList.AddRange(vertices); }
                else
                {
                    if (attachtype == 1 || attachtype == 3) { vertices.Reverse(); }
                    if (attachtype == 0 || attachtype == 1)
                    {
                        verticeList.AddRange(vertices.GetRange(1, vertices.Count - 1));
                    }
                    if (attachtype == 2 || attachtype == 3)
                    {
                        verticeList = vertices.Concat((List<Vector3>)verticeList.GetRange(1, verticeList.Count - 1)).ToList();
                    }
                    if (attachtype == 5)
                    {
                        verticeList.AddRange(vertices);
                        messFlag = true;
                    }
                }
            }

            if (rectBox.IsNull())
            {
                rectBox = rectbox;
            }
            else
            {
                rectBox.Unin(rectbox);
            }
        }


        public bool isClosed()
        {
            if (Helper.IsSameVertice(verticeList.First(), verticeList.Last(), Helper.CONNECT_TOLERANCE))
            { return true; }
            else
            { return false; }
        }

        public bool isMessLoop()
        {
            return messFlag;
        }


        public Vector3 LastVertice()
        {
            return verticeList.Last();
        }

        public Vector3 FirstVertice()
        {
            return verticeList.First();
        }

        public RectBox GetRectBox()
        {
            return rectBox;
        }

        public List<Vector3> GetVertices()
        {
            return verticeList;
        }
    }
}
