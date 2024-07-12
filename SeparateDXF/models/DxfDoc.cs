using netDxf.Header;
using netDxf;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.IO;
using System.Collections;
using netDxf.Entities;
using SeparateDXF.models;
using netDxf.Collections;
using netDxf.Tables;

namespace SeparateDXF.models
{
    internal class DxfDoc
    {
        private DxfDocument? loadeddxf;
        

        public DxfDoc()
        {

        }
        public bool LoadDxf(string fpath)
        {
            string dxfFPath = fpath;
            if (!File.Exists(dxfFPath)) { return false; }

            // this check is optional but recommended before loading a DXF file
            DxfVersion dxfVersion = DxfDocument.CheckDxfFileVersion(dxfFPath);
            // netDxf is only compatible with AutoCad2000 and higher DXF version
            if (dxfVersion < DxfVersion.AutoCad2000)
            {
                MessageBox.Show("Dxf file version is lower than ACAD2000, not support!", "Error");
                return false;
            }

            // load file
            loadeddxf = DxfDocument.Load(dxfFPath);
            if (loadeddxf != null) { return true; }
            else { return false; }
        }

        public TextStyles DxfStyles()
        {
            return loadeddxf.TextStyles;
        }


        public EntityItemCollect CollectAllEntity()
        {
            EntityItemCollect entityItemCollect = new EntityItemCollect();
            if (loadeddxf != null)
            {
                //parse the eneitties in the dxf file
                if (loadeddxf.Entities.Lines.Count() > 0) { CollectEntitys(entityItemCollect,  loadeddxf.Entities.Lines); }
                if (loadeddxf.Entities.Arcs.Count() > 0) { CollectEntitys(entityItemCollect, loadeddxf.Entities.Arcs); }
                if (loadeddxf.Entities.Circles.Count() > 0) { CollectEntitys(entityItemCollect, loadeddxf.Entities.Circles); }
                if (loadeddxf.Entities.Polylines2D.Count() > 0) { CollectEntitys(entityItemCollect, loadeddxf.Entities.Polylines2D); }
                if (loadeddxf.Entities.Splines.Count() > 0) { CollectEntitys(entityItemCollect, loadeddxf.Entities.Splines); }
                if (loadeddxf.Entities.Texts.Count() > 0) { CollectEntitys(entityItemCollect, loadeddxf.Entities.Texts); }
                if (loadeddxf.Entities.MTexts.Count() > 0) { CollectEntitys(entityItemCollect, loadeddxf.Entities.MTexts); }
            }
            return entityItemCollect;
        }


        private void CollectEntitys(EntityItemCollect entitycollection, IEnumerable<EntityObject> entities)
        {
            foreach (EntityObject entity in entities)
            {
                entitycollection.AttachEntity(entity);
            }
        }



        public List<LoopItem> GenerateLoopItemsfromEntitys(EntityItemCollect entitycollection)
        {
            List<LoopItem> loopItemList = new List<LoopItem>();
            //prepare the flag for loop identification
            bool[] linearcItemAssigned = new bool[entitycollection.LinearcList.Count];
            bool[] circleItemAssigned = new bool[entitycollection.CircleList.Count];
            bool[] polylineItemAssigned = new bool[entitycollection.PolylineList.Count];
            bool[] splineItemAssigned = new bool[entitycollection.SplineList.Count];


            //create depend loop item for circle and closed polyline and spline
            for (int i = 0; i < entitycollection.CircleList.Count; i++)
            {
                LoopItem newloopitem = new LoopItem();
                AttachEntity2Loop(newloopitem, entitycollection, EntityType.Circle, i, 0);
                circleItemAssigned[i] = true;
                loopItemList.Add(newloopitem);
            }

            if (entitycollection.PolylineList.Count > 0)
            {
                for (int i = 0; i < entitycollection.PolylineList.Count; i++)
                {
                    if (((Polyline2D)entitycollection.PolylineList[i].Entity).IsClosed == true)
                    {
                        LoopItem newloopitem = new LoopItem();
                        AttachEntity2Loop(newloopitem, entitycollection, EntityType.Polyline2D, i, 0);
                        polylineItemAssigned[i] = true;
                        loopItemList.Add(newloopitem);
                    }
                }
            }


            if (entitycollection.SplineList.Count > 0)
            {
                for (int i = 0; i < entitycollection.SplineList.Count; i++)
                {
                    if (((Spline)entitycollection.SplineList[i].Entity).IsClosed == true)
                    {
                        LoopItem newloopitem = new LoopItem();
                        AttachEntity2Loop(newloopitem, entitycollection, EntityType.Spline, i, 0);
                        splineItemAssigned[i] = true;
                        loopItemList.Add(newloopitem);
                    }
                }
            }


            //create loop for line arc and polyline left
            bool bNewLoopCreated = false;
            do
            {
                bNewLoopCreated = false;
                LoopItem newloopitem = new LoopItem();

                //get the first enetity of the new loop item
                if (bNewLoopCreated == false)
                {
                    //find the first entity not assigned as the first one in the loop
                    for (int i = 0; i < entitycollection.LinearcList.Count; i++)
                    {
                        if (linearcItemAssigned[i] == false) //not assigned
                        {
                            AttachEntity2Loop(newloopitem, entitycollection, EntityType.Line, i, 0);
                            linearcItemAssigned[i] = true;
                            bNewLoopCreated = true;
                            break;
                        }

                    }
                }
                if (bNewLoopCreated == false)
                {
                    //find the first entity not assigned as the first one in the loop
                    for (int i = 0; i < entitycollection.PolylineList.Count; i++)
                    {
                        if (polylineItemAssigned[i] == false) //not assigned
                        {
                            AttachEntity2Loop(newloopitem, entitycollection, EntityType.Polyline2D, i, 0);
                            polylineItemAssigned[i] = true;
                            bNewLoopCreated = true;
                            break;
                        }
                    }
                }
                if (bNewLoopCreated == false)
                {
                    //find the first entity not assigned as the first one in the loop
                    for (int i = 0; i < entitycollection.SplineList.Count; i++)
                    {
                        if (splineItemAssigned[i] == false) //not assigned
                        {
                            AttachEntity2Loop(newloopitem, entitycollection, EntityType.Spline, i, 0);
                            splineItemAssigned[i] = true;
                            bNewLoopCreated = true;
                            break;
                        }
                    }
                }


                //look for connected entities for the created new loop
                if (bNewLoopCreated == true)
                {
                    bool bFoundNewEntity = false;
                    do
                    {
                        bFoundNewEntity = false;
                        for (int ii = 0; ii < entitycollection.LinearcList.Count; ii++)
                        {
                            if (linearcItemAssigned[ii] == false)
                            {
                                EntityObject entity = entitycollection.LinearcList[ii].Entity;
                                List<Vector3> entityVertices = entitycollection.LinearcList[ii].Vertices;
                                RectBox entityRectBox = entitycollection.LinearcList[ii].Rectbox;


                                //compare with the end vertice of the new loop item first
                                if (Helper.IsSameVertice(entityVertices.First(), newloopitem.LastVertice()))
                                {
                                    AttachEntity2Loop(newloopitem, entitycollection, EntityType.Line, ii, 0);
                                    linearcItemAssigned[ii] = true;
                                    bFoundNewEntity = true;
                                    continue;
                                }
                                if (Helper.IsSameVertice(entityVertices.First(), newloopitem.FirstVertice()))
                                {
                                    AttachEntity2Loop(newloopitem, entitycollection, EntityType.Line, ii, 3);
                                    linearcItemAssigned[ii] = true;
                                    bFoundNewEntity = true;
                                    continue;
                                }
                                if (Helper.IsSameVertice(entityVertices.Last(), newloopitem.LastVertice()))
                                {
                                    AttachEntity2Loop(newloopitem, entitycollection, EntityType.Line, ii, 1);
                                    linearcItemAssigned[ii] = true;
                                    bFoundNewEntity = true;
                                    continue;
                                }
                                if (Helper.IsSameVertice(entityVertices.Last(), newloopitem.FirstVertice()))
                                {
                                    AttachEntity2Loop(newloopitem, entitycollection, EntityType.Line, ii, 2);
                                    linearcItemAssigned[ii] = true;
                                    bFoundNewEntity = true;
                                    continue;
                                }



                                //make further compare
                                if (entityRectBox.Iscross(newloopitem.GetRectBox(), Helper.BOXCROSS_TOLERANCE))
                                {
                                    List<Vector3> loopvertices = newloopitem.GetVertices();
                                    for (int jj = 0; jj < loopvertices.Count; jj++)
                                    {
                                        if (Helper.IsSameVertice(loopvertices[jj], entityVertices.First()) ||
                                           Helper.IsSameVertice(loopvertices[jj], entityVertices.Last())
                                        )
                                        {
                                            AttachEntity2Loop(newloopitem, entitycollection, EntityType.Line, ii, 5);
                                            linearcItemAssigned[ii] = true;
                                            bFoundNewEntity = true;
                                            break;
                                        }
                                    }
                                }

                            }
                        }

                        //process the polyline
                        for (int ii = 0; ii < entitycollection.PolylineList.Count; ii++)
                        {
                            if (polylineItemAssigned[ii] == false)
                            {
                                EntityObject entity = entitycollection.PolylineList[ii].Entity;
                                List<Vector3> entityVertices = entitycollection.PolylineList[ii].Vertices;
                                RectBox entityRectBox = entitycollection.PolylineList[ii].Rectbox;


                                //compare with the end vertice of the new loop item first
                                if (Helper.IsSameVertice(entityVertices.First(), newloopitem.LastVertice()))
                                {
                                    AttachEntity2Loop(newloopitem, entitycollection, EntityType.Polyline2D, ii, 0);
                                    polylineItemAssigned[ii] = true;
                                    bFoundNewEntity = true;
                                    continue;
                                }
                                if (Helper.IsSameVertice(entityVertices.First(), newloopitem.FirstVertice()))
                                {
                                    AttachEntity2Loop(newloopitem, entitycollection, EntityType.Polyline2D, ii, 3);
                                    polylineItemAssigned[ii] = true;
                                    bFoundNewEntity = true;
                                    continue;
                                }
                                if (Helper.IsSameVertice(entityVertices.Last(), newloopitem.LastVertice()))
                                {
                                    AttachEntity2Loop(newloopitem, entitycollection, EntityType.Polyline2D, ii, 1);
                                    polylineItemAssigned[ii] = true;
                                    bFoundNewEntity = true;
                                    continue;
                                }
                                if (Helper.IsSameVertice(entityVertices.Last(), newloopitem.FirstVertice()))
                                {
                                    AttachEntity2Loop(newloopitem, entitycollection, EntityType.Polyline2D, ii, 2);
                                    polylineItemAssigned[ii] = true;
                                    bFoundNewEntity = true;
                                    continue;
                                }



                                //make further compare
                                if (entityRectBox.Iscross(newloopitem.GetRectBox()))
                                {
                                    List<Vector3> loopvertices = newloopitem.GetVertices();
                                    for (int jj = 0; jj < loopvertices.Count; jj++)
                                    {
                                        if (Helper.IsSameVertice(loopvertices[jj], entityVertices.First()) ||
                                           Helper.IsSameVertice(loopvertices[jj], entityVertices.Last())
                                        )
                                        {
                                            AttachEntity2Loop(newloopitem, entitycollection, EntityType.Polyline2D, ii, 5);
                                            polylineItemAssigned[ii] = true;
                                            bFoundNewEntity = true;
                                            break;
                                        }
                                    }
                                }

                            }
                        }


                        //process the spline
                        for (int ii = 0; ii < entitycollection.SplineList.Count; ii++)
                        {
                            if (splineItemAssigned[ii] == false)
                            {
                                EntityObject entity = entitycollection.SplineList[ii].Entity;
                                List<Vector3> entityVertices = entitycollection.SplineList[ii].Vertices;
                                RectBox entityRectBox = entitycollection.SplineList[ii].Rectbox;


                                //compare with the end vertice of the new loop item first
                                if (Helper.IsSameVertice(entityVertices.First(), newloopitem.LastVertice()))
                                {
                                    AttachEntity2Loop(newloopitem, entitycollection, EntityType.Spline, ii, 0);
                                    splineItemAssigned[ii] = true;
                                    bFoundNewEntity = true;
                                    continue;
                                }
                                if (Helper.IsSameVertice(entityVertices.First(), newloopitem.FirstVertice()))
                                {
                                    AttachEntity2Loop(newloopitem, entitycollection, EntityType.Spline, ii, 3);
                                    splineItemAssigned[ii] = true;
                                    bFoundNewEntity = true;
                                    continue;
                                }
                                if (Helper.IsSameVertice(entityVertices.Last(), newloopitem.LastVertice()))
                                {
                                    AttachEntity2Loop(newloopitem, entitycollection, EntityType.Spline, ii, 1);
                                    splineItemAssigned[ii] = true;
                                    bFoundNewEntity = true;
                                    continue;
                                }
                                if (Helper.IsSameVertice(entityVertices.Last(), newloopitem.FirstVertice()))
                                {
                                    AttachEntity2Loop(newloopitem, entitycollection, EntityType.Spline, ii, 2);
                                    splineItemAssigned[ii] = true;
                                    bFoundNewEntity = true;
                                    continue;
                                }



                                //make further compare
                                if (entityRectBox.Iscross(newloopitem.GetRectBox()))
                                {
                                    List<Vector3> loopvertices = newloopitem.GetVertices();
                                    for (int jj = 0; jj < loopvertices.Count; jj++)
                                    {
                                        if (Helper.IsSameVertice(loopvertices[jj], entityVertices.First()) ||
                                           Helper.IsSameVertice(loopvertices[jj], entityVertices.Last())
                                        )
                                        {
                                            AttachEntity2Loop(newloopitem, entitycollection, EntityType.Spline, ii, 5);
                                            splineItemAssigned[ii] = true;
                                            bFoundNewEntity = true;
                                            break;
                                        }
                                    }
                                }

                            }
                        }


                    } while (bFoundNewEntity);

                }

                if (bNewLoopCreated)
                {
                    loopItemList.Add(newloopitem);
                }


            } while (bNewLoopCreated);

            return loopItemList;
        }

        private void AttachEntity2Loop(LoopItem desLoopItem, EntityItemCollect srcentitycollection, EntityType type, int entityIdx, int attachtype = 0)
        {
            switch (type)
            {
                case EntityType.Line:
                case EntityType.Arc:
                    desLoopItem.AttachGeomItem(srcentitycollection.LinearcList[entityIdx].Entity, srcentitycollection.LinearcList[entityIdx].Vertices, srcentitycollection.LinearcList[entityIdx].Rectbox, attachtype);
                    break;
                case EntityType.Circle:
                    desLoopItem.AttachGeomItem(srcentitycollection.CircleList[entityIdx].Entity, srcentitycollection.CircleList[entityIdx].Vertices, srcentitycollection.CircleList[entityIdx].Rectbox, attachtype);
                    break;
                case EntityType.Polyline2D:
                    desLoopItem.AttachGeomItem(srcentitycollection.PolylineList[entityIdx].Entity, srcentitycollection.PolylineList[entityIdx].Vertices, srcentitycollection.PolylineList[entityIdx].Rectbox, attachtype);
                    break;
                case EntityType.Spline:
                    desLoopItem.AttachGeomItem(srcentitycollection.SplineList[entityIdx].Entity, srcentitycollection.SplineList[entityIdx].Vertices, srcentitycollection.SplineList[entityIdx].Rectbox, attachtype);
                    break;
            }
        }




        public List<LoopTopItem> GenerateLoopTopItemsfromLoopList(List<LoopItem> loopitemlist)
        {
            List<LoopTopItem> loopTopItemList = new List<LoopTopItem>();
            foreach (LoopItem loopitem in loopitemlist)
            {
                LoopTopItem loopTopItem = new LoopTopItem(loopitem);
                if (loopTopItemList.Count == 0)
                {
                    loopTopItemList.Add(loopTopItem);
                }
                else
                {
                    AddLoopTopItem2LoopTopList(loopTopItemList, loopTopItem);
                }
            }
            return loopTopItemList;
        }

        private void AddLoopTopItem2LoopTopList(List<LoopTopItem> loopTopItemList, LoopTopItem loopTopItem)
        {
            //flag, check if the looptopitem could be included in other looptopitem assigned
            bool bIncluded = false;

            //只有具有唯一封闭轮廓的图形才能包含其他轮廓
            //检查待分配轮廓是否包含已分配轮廓
            if (loopTopItem.cadLoopItem.isClosed() == true && loopTopItem.cadLoopItem.isMessLoop() == false)
            {
                for (int i = loopTopItemList.Count - 1; i >= 0; i--)
                {

                    LoopItem assignedCadLoopItem = loopTopItemList[i].cadLoopItem;
                    LoopItem cadLoopItem = loopTopItem.cadLoopItem;
                    if (cadLoopItem.GetRectBox().Include(assignedCadLoopItem.GetRectBox()))
                    {
                        //如果是开发轮廓，直接包含处理
                        if (assignedCadLoopItem.isClosed() == false || loopTopItem.cadLoopItem.isMessLoop() == true)
                        {
                            bIncluded = true;
                            loopTopItem.subLoopTopItemList.Add(loopTopItemList[i]);
                            loopTopItemList.RemoveAt(i);
                        }
                        else
                        {
                            if (Helper.CheckLoopVerticesInclude(cadLoopItem.GetVertices(), assignedCadLoopItem.GetVertices()))
                            {
                                bIncluded = true;
                                loopTopItem.subLoopTopItemList.Add(loopTopItemList[i]);
                                loopTopItemList.RemoveAt(i);
                            }
                        }
                    }
                }
            }

            bool bBeIncluded = false;
            if (!bIncluded)
            {
                foreach (LoopTopItem assignedLoopTopItem in loopTopItemList)
                {
                    //检查已分配轮廓矩形框是否包含待分配轮廓矩形框
                    if (assignedLoopTopItem.cadLoopItem.GetRectBox().Include(loopTopItem.cadLoopItem.GetRectBox()))
                    {
                        //进一步检查是否精确包含
                        if (Helper.CheckLoopVerticesInclude(assignedLoopTopItem.cadLoopItem.GetVertices(), loopTopItem.cadLoopItem.GetVertices()))
                        {
                            bBeIncluded = true;

                            AddLoopTopItem2LoopTopList(assignedLoopTopItem.subLoopTopItemList, loopTopItem);
                        }
                    }
                }
            }

            if (!bBeIncluded)
            {
                loopTopItemList.Add(loopTopItem);
            }
        }




        public void BuildPartListfromLoopTopList(List<LoopTopItem> looptoplist, ref List<List<EntityObject>> partSepList, ref List<RectBox> partRectBoxList)
        {
            for (int i = 0; i < looptoplist.Count; i++)
            {
                LoopTopItem looptopitem = looptoplist[i];
                if (looptopitem.cadLoopItem.isClosed() == true)
                {
                    List<EntityObject> partEntityList = looptopitem.cadLoopItem.loopEntityList;
                    RectBox partRectBox = looptopitem.cadLoopItem.GetRectBox();

                    List<LoopTopItem> sublooptoplist = looptopitem.subLoopTopItemList;
                    for (int j = 0; j < sublooptoplist.Count; j++)
                    {
                        LoopTopItem sublooptopitem = sublooptoplist[j];
                        partEntityList.AddRange(sublooptopitem.cadLoopItem.loopEntityList);
                        partRectBox.Unin(sublooptopitem.cadLoopItem.GetRectBox());

                        BuildPartListfromLoopTopList(sublooptopitem.subLoopTopItemList, ref partSepList, ref partRectBoxList);
                    }

                    partSepList.Add(partEntityList);
                    partRectBoxList.Add(partRectBox);
                }
            }
        }



        public void SeparateText2PrtList(EntityItemCollect entitycollection, ref List<RectBox> partRectBoxList, ref List<List<EntityObject>> partSepList)
        {

            if (entitycollection.TextList.Count > 0)
            {
                List<EntityItem> textList = entitycollection.TextList;
                for (int i = 0; i < textList.Count; i++)
                {
                    EntityItem text = textList[i];
                    for (int j = 0; j < partRectBoxList.Count; j++)
                    {
                        if (partRectBoxList[j].Iscross(text.Rectbox))
                        {
                            partSepList[j].Add(text.Entity);
                        }
                    }
                }
            }

            if (entitycollection.MtextList.Count > 0)
            {
                List<EntityItem> mtextList = entitycollection.MtextList;
                for (int i = 0; i < mtextList.Count; i++)
                {
                    EntityItem mtext = mtextList[i];
                    for (int j = 0; j < partRectBoxList.Count; j++)
                    {
                        if (partRectBoxList[j].Iscross(mtext.Rectbox))
                        {
                            partSepList[j].Add(mtext.Entity);
                        }
                    }
                }
            }

        }
    }
}
