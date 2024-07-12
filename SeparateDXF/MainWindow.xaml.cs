using Microsoft.Win32;
using netDxf;
using netDxf.Entities;
using netDxf.Header;
using netDxf.Tables;
using SeparateDXF.models;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Security.Policy;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;


namespace SeparateDXF
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();
        }

        private void onSeperateDxf(object sender, RoutedEventArgs e)
        {

            string filePath = string.Empty;

            OpenFileDialog openFileDialog = new OpenFileDialog();

            openFileDialog.Filter = "dxf files (*.dxf)|*.dxf";
            openFileDialog.RestoreDirectory = true;

            if (openFileDialog.ShowDialog() == true)
            {
                //Get the path of specified file
                filePath = openFileDialog.FileName;
                DxfDoc dxfDoc = new DxfDoc();
                if (dxfDoc.LoadDxf(filePath))
                {
                    EntityItemCollect allEntitys = dxfDoc.CollectAllEntity();
                    List<LoopItem> loopList = dxfDoc.GenerateLoopItemsfromEntitys(allEntitys);
                    List<LoopTopItem> loopTopList = dxfDoc.GenerateLoopTopItemsfromLoopList(loopList);

                    List<List<EntityObject>> partSepList = new List<List<EntityObject>>();
                    List<RectBox> partRectBoxList = new List<RectBox>();
                    dxfDoc.BuildPartListfromLoopTopList(loopTopList, ref partSepList, ref partRectBoxList);

                    if (partRectBoxList.Count > 1)
                    {
                        dxfDoc.SeparateText2PrtList(allEntitys, ref partRectBoxList, ref partSepList);
                    }


                    if (partSepList.Count > 1)
                    {
                        string dxfbasename = System.IO.Path.GetFileNameWithoutExtension(filePath);
                        string dxfpath = System.IO.Path.GetDirectoryName(filePath);  //GetFullPath(filePath);
                        //create the seperated part

                        for (int i = 0; i < partSepList.Count; i++)
                        {
                            string dxffullname = dxfpath+"\\"+dxfbasename + "_" + (i + 1) + ".dxf";
                            List<EntityObject> subprt = partSepList[i];
                            //build and save dxf part
                            DxfDocument prtdxfdoc = new DxfDocument(DxfVersion.AutoCad2010);

                            foreach (TextStyle style in dxfDoc.DxfStyles())
                            {
                                if (prtdxfdoc.TextStyles.Contains(style.Name)) 
                                {
                                    if (!prtdxfdoc.TextStyles.Remove(style.Name))
                                    {
                                        //"Standard" text style is reserved in netdxf and could not be removed.
                                        //update the textstyle info
                                        prtdxfdoc.TextStyles[style.Name].FontStyle = style.FontStyle;
                                        if (!string.IsNullOrEmpty(style.FontFile)) prtdxfdoc.TextStyles[style.Name].FontFile = style.FontFile;
                                        if (!string.IsNullOrEmpty(style.BigFont)) prtdxfdoc.TextStyles[style.Name].BigFont = style.BigFont;
                                        if(!string.IsNullOrEmpty(style.FontFamilyName)) prtdxfdoc.TextStyles[style.Name].FontFamilyName = style.FontFamilyName;
                                        prtdxfdoc.TextStyles[style.Name].Height = style.Height;
                                        prtdxfdoc.TextStyles[style.Name].ObliqueAngle = style.ObliqueAngle;
                                        prtdxfdoc.TextStyles[style.Name].WidthFactor = style.WidthFactor;
                                    }
                                }
                                prtdxfdoc.TextStyles.Add(style);
                            }

                            foreach (EntityObject item in subprt)
                            {
                                prtdxfdoc.Entities.Add((EntityObject)item.Clone());
                            }

                            prtdxfdoc.Save(dxffullname);
                        }
                        MessageBox.Show("Multiple DXF part files generated!", "Finished");
                    }
                    else
                    {
                        MessageBox.Show("One Part only in the DXF file!", "Finished");
                    }
                }
            }
        }
    }
}
