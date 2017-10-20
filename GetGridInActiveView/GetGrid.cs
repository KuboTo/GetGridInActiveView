using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Globalization;
using System.Linq;
using System.Security.Cryptography.X509Certificates;
using System.Windows.Forms;
using Autodesk.Revit.UI;
using Autodesk.Revit.DB;
using Autodesk.Revit.Attributes;
using Autodesk.Revit.UI.Selection;
using Autodesk.Revit.DB.Structure;
using Application = Autodesk.Revit.ApplicationServices.Application;
using View = Autodesk.Revit.DB.View;

namespace GetGridInActiveView
{
    [Transaction(TransactionMode.Manual)]
    [Regeneration(RegenerationOption.Manual)]
    public class GetGrid : IExternalCommand
    {
        public Result Execute(ExternalCommandData commandData, ref string message, ElementSet elements)
        {

            UIApplication uiApp = commandData.Application;
            Autodesk.Revit.ApplicationServices.Application app = uiApp.Application;
            Document document = uiApp.ActiveUIDocument.Document;
            Selection sel = uiApp.ActiveUIDocument.Selection;

            Transaction trans = new Transaction(document, "过滤获取当前视图中的轴网信息");
            trans.Start();

            FilteredElementCollector collector = new FilteredElementCollector(document, document.ActiveView.Id);//
            collector.OfClass(typeof(Grid));

            string sInfo = null;
            foreach (Element elem in collector)
            {
                sInfo += "Name = " + elem.Name + ";";

                Grid grid = elem as Grid;
                LocationCurve locCurve = grid.Location as LocationCurve;
                if (locCurve != null)
                {
                    Curve cur = locCurve.Curve;

                    XYZ ptStart = cur.GetEndPoint(0);
                    XYZ ptEnd = cur.GetEndPoint(1);
                }
            }

            TaskDialog.Show("Grid message", sInfo);

            trans.Commit();

            return Result.Succeeded;
        }

        #region 创建视图

        [Transaction(TransactionMode.Manual)]
        [Regeneration(RegenerationOption.Manual)]
        public class CreateViewCallout : IExternalCommand
        {

            public Result Execute(ExternalCommandData commandData, ref string message, ElementSet elements)
            {
                UIApplication uiApp = commandData.Application;
                Application app = uiApp.Application;
                Document document = uiApp.ActiveUIDocument.Document;
                Selection sel = uiApp.ActiveUIDocument.Selection;
                UIDocument uiDoc = commandData.Application.ActiveUIDocument;

                Transaction trans = new Transaction(document, "创建索引详图");
                trans.Start();

                #region 方案一

                XYZ p1 = sel.PickPoint();

                XYZ p2 = sel.PickPoint();


                //创建新视图所需要的ViewFamilyType的Id
                ElementId viewTypeId = document.ActiveView.GetTypeId();
                //获得当前视图即为父视图ElementId
                View view = ViewSection.CreateCallout(document, document.ActiveView.Id, viewTypeId, p1,
                    p2);
                //ViewPlan detViewPlan = ViewSection.CreateCallout(document, document.ActiveView.Id, viewTypeId, p1, p2) as ViewPlan;


                #endregion

                trans.Commit();

                //document.ActiveView = view;
                uiDoc.ActiveView = view;

                //增加删除刚才创建的索引详图功能？？？

                //导出索引详图视图截图
                ImageExportOptions options = new ImageExportOptions();
                options.ZoomType = ZoomFitType.FitToPage;
                options.ExportRange = ExportRange.CurrentView;
                options.FilePath = @"C:\Users\Administrator.WIN7U-20141230O\Desktop\map\CurrentViewImage";//命名需要修改，不然每次重复替换
                options.FitDirection = FitDirectionType.Horizontal;
                options.HLRandWFViewsFileType = ImageFileType.JPEGMedium;
                options.ShadowViewsFileType = ImageFileType.JPEGMedium;
                options.PixelSize = 1920;
                commandData.Application.ActiveUIDocument.Document.ExportImage(options);


                #region 切换三维视图(未成功，可能版本问题）

                //        Transaction tr = new Transaction(document, "切换三维视图");
                //        tr.Start();
                //        // 找到一个三维视图类型
                //        IEnumerable<ViewFamilyType> viewFamilyTypes =
                //            from elem in new FilteredElementCollector(document).OfClass(typeof (ViewFamilyType))
                //            let type = elem as ViewFamilyType
                //            where type.ViewFamily = ViewFamily.ThreeDimensional
                //            select type;
                //        //创建三维视图
                //        View3D view3D = View3D.CreatePerspective(document, viewFamilyTypes.First().Id);

                //        tr.Commit();

                //        uiDoc.ActiveView = view3D;

                #endregion



                return Result.Succeeded;
            }

            #endregion

        }
    }
}
