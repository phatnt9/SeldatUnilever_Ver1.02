﻿using Newtonsoft.Json;
using SeldatUnilever_Ver1._02.DTO;
using System;
using System.Collections.Generic;
using System.Data;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Shapes;
namespace SeldatUnilever_Ver1._02.Shape
{
    public class CanvasControlService
    {
        //=================VARIABLE==================
        private int stationCount = 0;
        //---------------MAP-------------------
        private MainWindow mainWindow;
        private Canvas map;
        private ScaleTransform scaleTransform;
        private TranslateTransform translateTransform;
        private TreeView mainTreeView;
        //---------------DRAW-------------------
        //private Point roundedMousePos = new Point();
        private Point startPoint;
        private Point draw_StartPoint;
        private Point originalPoint;
        //CanvasPath pathTemp;
        //---------------POINT OF VIEW-------------------
        private string selectedItemName = "";
        private string hoveringItemName = "";
        private double slidingScale;

        //---------------OBJECT-------------------
        //public SortedDictionary<string, CanvasPath> list_Path;
        public SortedDictionary<string, StationShape> list_Station;
        public SortedDictionary<string, RobotShape> list_Robot;
        //double yDistanceBottom, xDistanceLeft, yDistanceTop, xDistanceRight;
        //---------------MICS-------------------
        //private Ellipse cursorPoint = new Ellipse();
        //======================MAP======================
        //public CanvasControlService(MainWindow mainWinDowIn, TreeView mainTreeViewIn)
        public CanvasControlService(MainWindow mainWinDowIn)
        {
            mainWindow = mainWinDowIn;
            //mainTreeView = mainTreeViewIn;
            map = mainWindow.map;
            scaleTransform = mainWindow.canvasScaleTransform;
            translateTransform = mainWindow.canvasTranslateTransform;
            //list_Path = new SortedDictionary<string, CanvasPath>();
            list_Station = new SortedDictionary<string, StationShape>();
            list_Robot = new SortedDictionary<string, RobotShape>();
            //==========EVENT==========
            map.MouseDown += Map_MouseDown;
            map.MouseWheel += Map_Zoom;
            map.MouseMove += Map_MouseMove;
            map.SizeChanged += Map_SizeChanged;
            map.MouseLeftButtonDown += Map_MouseLeftButtonDown;
            map.MouseRightButtonDown += Map_MouseRightButtonDown;
            map.MouseLeftButtonUp += Map_MouseLeftButtonUp;
            mainWindow.PreviewKeyDown += new KeyEventHandler(HandleEsc);
            mainWindow.clipBorder.SizeChanged += ClipBorder_SizeChanged;

        }

        //########################################################
        //METHOD======METHOD======METHOD======METHOD======METHOD==
        //########################################################

        private void ToggleSelectedPath(string currentPath)
        {
            //if (list_Path.ContainsKey(currentPath))
            //{
            //    list_Path[currentPath].props.isSelected = false;
            //    list_Path[currentPath].ToggleStyle();

            //}
        }
        private void ReCenterMapCanvas()
        {
            double MapWidthScaled = (map.Width * scaleTransform.ScaleX);
            double MapHeightScaled = (map.Height * scaleTransform.ScaleY);
            double ClipBorderWidth = (mainWindow.clipBorder.ActualWidth);
            double ClipBorderHeight = (mainWindow.clipBorder.ActualHeight);

            double xlim;
            double ylim;
            //==========================================================
            if (ClipBorderWidth < map.Width)
            {
                xlim = (map.Width * (scaleTransform.ScaleX - 1)) / 2;
            }
            else
            {
                xlim = Math.Abs((MapWidthScaled - ClipBorderWidth) / 2);
            }

            if (ClipBorderHeight < map.Height)
            {
                ylim = (map.Height * (scaleTransform.ScaleY - 1)) / 2;
            }
            else
            {
                ylim = Math.Abs((MapHeightScaled - ClipBorderHeight) / 2);
            }
            //==========================================================
            if (ClipBorderWidth > map.Width)
            {
                translateTransform.X = 0;
            }
            else
            {
                translateTransform.X = ((xlim) - (MapWidthScaled - ClipBorderWidth - xlim)) / 2;
            }
            if (ClipBorderHeight > map.Height)
            {
                translateTransform.Y = 0;
            }
            else
            {
                translateTransform.Y = ((ylim) - (MapHeightScaled - ClipBorderHeight - ylim)) / 2;
            }
        }

        //////////////////////////////////////////////////////
        //EVENT========EVENT========EVENT========EVENT========
        //////////////////////////////////////////////////////

        private void ClipBorder_SizeChanged(object sender, SizeChangedEventArgs e)
        {
            //Console.WriteLine("ClipBorder_SizeChanged");
            ReCenterMapCanvas();
        }
        private void Map_MouseDown(object sender, MouseButtonEventArgs e)
        {
            string elementName = (e.OriginalSource as FrameworkElement).Name;
        }
        private void Map_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            string elementName = (e.OriginalSource as FrameworkElement).Name;
            ToggleSelectedPath(selectedItemName);
            selectedItemName = elementName;
            Console.WriteLine(elementName);
            if ((mainWindow.drag))
            {
                if (e.Source.ToString() == "System.Windows.Controls.Canvas")
                {
                    map.CaptureMouse();
                    startPoint = e.GetPosition(mainWindow.clipBorder);
                    originalPoint = new Point(translateTransform.X, translateTransform.Y);
                }
            }
            if (!mainWindow.drag)
            {
                Statectrl_MouseDown(e);
            }
        }
        private void Map_MouseRightButtonDown(object sender, MouseButtonEventArgs e)
        {
            string elementName = (e.OriginalSource as FrameworkElement).Name;
            //selectedItemName = elementName;
        }
        private void Map_Zoom(object sender, MouseWheelEventArgs e)
        {
            Point mousePos = e.GetPosition(map);
            double zoomDirection = e.Delta > 0 ? 1 : -1;
            slidingScale = 0.1 * zoomDirection;
            double edgeW = (scaleTransform.ScaleX + slidingScale) * map.Width;
            double edgeH = (scaleTransform.ScaleY + slidingScale) * map.Height;
            if (((edgeW > 1) || (edgeH > 1)) && ((edgeW < (map.Width * 10)) || (edgeH < (map.Height * 10))))
            {
                scaleTransform.ScaleX = scaleTransform.ScaleY += slidingScale;
            }
            ReCenterMapCanvas();
        }
        private void Map_SizeChanged(object sender, SizeChangedEventArgs e)
        {
            Global_Object.OriginPoint.X = map.Width * 0.5;
            Global_Object.OriginPoint.Y = map.Height * 0.5;
            mainWindow.rect.RenderTransform = new TranslateTransform(Global_Object.OriginPoint.X, Global_Object.OriginPoint.Y);
            //mainWindow.robot.RenderTransform = new TranslateTransform(Global_Object.OriginPoint.X, Global_Object.OriginPoint.Y);
            Point backGroundTransform = Global_Object.LaserOriginalCoor;
            double X = Global_Object.OriginPoint.X - backGroundTransform.X;
            double Y = Global_Object.OriginPoint.Y - backGroundTransform.Y;
            map.Background.Transform = new TranslateTransform(X, Y);
            ReCenterMapCanvas();
        }
        private void Map_MouseMove(object sender, MouseEventArgs e)
        {
            //Get mouse props
            Point mousePos = e.GetPosition(map);
            var mouseWasDownOn = (e.Source as FrameworkElement);
            hoveringItemName = mouseWasDownOn.Name;
            //mainWindow.MouseCoor.Content = (mousePos.X- Global_Object.OriginPoint.X).ToString("0.0") + " " + (Global_Object.OriginPoint.Y - mousePos.Y ).ToString("0.0");
            mainWindow.MouseCoor.Content = (Global_Object.CoorLaser(mousePos).X).ToString("0.00") + " " + (Global_Object.CoorLaser(mousePos).Y).ToString("0.00");
            mainWindow.MouseCoor2.Content = ((mousePos).X).ToString("0.00") + " " + ((mousePos).Y).ToString("0.00");
            //mainWindow.MouseCoor.Content = ((mousePos).X.ToString("0.0") + " " + (mousePos).Y.ToString("0.0"));

            //Console.WriteLine("============================================");
            //Console.WriteLine("MousePos:  (" + mousePos.X + "," + mousePos.Y + ")");
            //Console.WriteLine("CoorLaser:  (" + Global_Object.CoorLaser(mousePos).X.ToString("0.0") + "," + Global_Object.CoorLaser(mousePos).Y.ToString("0.0")+")");
            //Console.WriteLine("CoorCanvas:  ("+ Global_Object.CoorCanvas(Global_Object.CoorLaser(mousePos)).X.ToString("0.0") + "," + Global_Object.CoorCanvas(Global_Object.CoorLaser(mousePos)).Y.ToString("0.0") + ")");
            ////
            // POINT OF VIEW
            //
            if ((mainWindow.drag))
            {
                if (!map.IsMouseCaptured) return;
                Vector moveVector = startPoint - e.GetPosition(mainWindow.clipBorder);
                double xCoor = originalPoint.X - moveVector.X;
                double yCoor = originalPoint.Y - moveVector.Y;

                double MapWidthScaled = (map.Width * scaleTransform.ScaleX);
                double MapHeightScaled = (map.Height * scaleTransform.ScaleY);
                double ClipBorderWidth = (mainWindow.clipBorder.ActualWidth);
                double ClipBorderHeight = (mainWindow.clipBorder.ActualHeight);

                double xlim;
                double ylim;
                if (ClipBorderWidth < map.Width)
                {
                    xlim = (map.Width * (scaleTransform.ScaleX - 1)) / 2;
                }
                else
                {
                    xlim = Math.Abs((MapWidthScaled - ClipBorderWidth) / 2);
                }

                if (ClipBorderHeight < map.Height)
                {
                    ylim = (map.Height * (scaleTransform.ScaleY - 1)) / 2;
                }
                else
                {
                    ylim = Math.Abs((MapHeightScaled - ClipBorderHeight) / 2);
                }

                if (ClipBorderWidth > map.Width)
                {
                    if ((xCoor >= (-xlim)) && (xCoor <= (xlim)))
                    {
                        translateTransform.X = xCoor;
                    }
                }
                else
                {
                    if (ClipBorderWidth < MapWidthScaled)
                    {
                        if ((xCoor <= (xlim)) && (xCoor >= -(MapWidthScaled - ClipBorderWidth - xlim)))
                        {
                            translateTransform.X = xCoor;
                        }
                    }
                    else
                    {
                        if ((xCoor >= (xlim)) && (xCoor <= -(MapWidthScaled - ClipBorderWidth - xlim)))
                        {
                            translateTransform.X = xCoor;
                        }
                    }
                }
                if (ClipBorderHeight > map.Height)
                {
                    if ((yCoor >= (-ylim)) && (yCoor <= (ylim)))
                    {
                        translateTransform.Y = yCoor;
                    }
                }
                else
                {
                    if (ClipBorderHeight < MapHeightScaled)
                    {
                        if ((yCoor <= (ylim)) && (yCoor >= -(MapHeightScaled - ClipBorderHeight - ylim)))
                        {
                            translateTransform.Y = yCoor;
                        }
                    }
                    else
                    {
                        if ((yCoor >= (ylim)) && (yCoor <= -(MapHeightScaled - ClipBorderHeight - ylim)))
                        {
                            translateTransform.Y = yCoor;
                        }
                    }
                }
            }
            if (!mainWindow.drag)
            {
                Statectrl_MouseMove(e);
            }

        }
        private void Map_MouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            map.ReleaseMouseCapture();
        }


        //////////////////////////////////////////////////////
        //PROCESS=====PROCESS=====PROCESS=====PROCESS=========
        //////////////////////////////////////////////////////
        private void HandleEsc(object sender, KeyEventArgs e)
        {
            switch (e.Key)
            {
                case Key.Escape:
                    {
                        Normal_mode();
                        break;
                    }
                case Key.Delete:
                    {
                        try
                        {
                            //if (list_Path.ContainsKey(selectedItemName))
                            //{
                            //    list_Path[selectedItemName].Remove();
                            //    //list_Path.Remove(selectedItemName);
                            //    Console.WriteLine("Remove: " + selectedItemName + "-Count: " + list_Path.Count);
                            //}
                        }
                        catch
                        {
                            Console.WriteLine("Nothing to remove");
                        }
                        break;
                    }
                default:
                    {
                        break;
                    }
            }
        }
        public void Normal_mode()
        {
            mainWindow.drag = true;
            Global_Mouse.ctrl_MouseMove = Global_Mouse.STATE_MOUSEMOVE._NORMAL;
            Global_Mouse.ctrl_MouseDown = Global_Mouse.STATE_MOUSEDOWN._NORMAL;
        }
        public void Select_mode()
        {
            //valstatectrl_mm = STATECTRL_MOUSEMOVE.STATECTRL_SLIDE_OBJECT;
            //valstatectrl_md = STATECTRL_MOUSEDOWN.STATECTRL_KEEP_IN_OBJECT;
        }
        void Statectrl_MouseDown(MouseButtonEventArgs e)
        {
            if (e.ClickCount == 2)
            {
                //EditObject();
            }
            Point mousePos = e.GetPosition(map);
            var mouseWasDownOn = e.Source as FrameworkElement;
            switch (Global_Mouse.ctrl_MouseDown)
            {
                case Global_Mouse.STATE_MOUSEDOWN._ADD_STATION:
                    {
                        if (Global_Mouse.ctrl_MouseDown == Global_Mouse.STATE_MOUSEDOWN._ADD_STATION)
                        {
                            StationShape stationTemp = null;
                            //stationTemp = new StationShape(map, "MIX" + stationCount, 2, 7, 0);
                            stationCount++;
                            stationTemp.Move(mousePos);
                            //map.Children.Add(stationTemp);
                        }
                        break;
                    }
                case Global_Mouse.STATE_MOUSEDOWN._KEEP_IN_OBJECT:
                    if (mouseWasDownOn != null)
                    {
                        string elementName = mouseWasDownOn.Name;
                        if (elementName != "")
                        {
                            Global_Mouse.ctrl_MouseMove = Global_Mouse.STATE_MOUSEMOVE._MOVE_STATION;
                            Global_Mouse.ctrl_MouseDown = Global_Mouse.STATE_MOUSEDOWN._GET_OUT_OBJECT;
                        }
                    }
                    break;
                case Global_Mouse.STATE_MOUSEDOWN._HAND_DRAW_STRAIGHT_P1:
                    //pathTemp = new StraightShape(map, new Point(0, 0), new Point(0, 0));
                    //pathTemp.RemoveHandle += PathRemove;
                    if (mouseWasDownOn != null)
                    {
                        string elementName = mouseWasDownOn.Name;
                        if (elementName != "")
                        {
                            draw_StartPoint = mousePos;
                            Global_Mouse.ctrl_MouseDown = Global_Mouse.STATE_MOUSEDOWN._HAND_DRAW_STRAIGHT_FINISH;
                            Global_Mouse.ctrl_MouseMove = Global_Mouse.STATE_MOUSEMOVE._HAND_DRAW_STRAIGHT;
                        }
                    }
                    break;
                case Global_Mouse.STATE_MOUSEDOWN._HAND_DRAW_CURVEUP_P1:
                    //pathTemp = new CurveShape(map, new Point(0, 0), new Point(0, 0), true);
                    if (mouseWasDownOn != null)
                    {
                        string elementName = mouseWasDownOn.Name;
                        if (elementName != "")
                        {
                            draw_StartPoint = mousePos;
                            Global_Mouse.ctrl_MouseDown = Global_Mouse.STATE_MOUSEDOWN._HAND_DRAW_CURVEUP_FINISH;
                            Global_Mouse.ctrl_MouseMove = Global_Mouse.STATE_MOUSEMOVE._HAND_DRAW_CURVE;
                        }
                    }
                    break;
                case Global_Mouse.STATE_MOUSEDOWN._HAND_DRAW_CURVEDOWN_P1:
                    //pathTemp = new CurveShape(map, new Point(0, 0), new Point(0, 0), false);
                    if (mouseWasDownOn != null)
                    {
                        string elementName = mouseWasDownOn.Name;
                        if (elementName != "")
                        {
                            draw_StartPoint = mousePos;
                            Global_Mouse.ctrl_MouseDown = Global_Mouse.STATE_MOUSEDOWN._HAND_DRAW_CURVEDOWN_FINISH;
                            Global_Mouse.ctrl_MouseMove = Global_Mouse.STATE_MOUSEMOVE._HAND_DRAW_CURVE;
                        }
                    }
                    break;
                case Global_Mouse.STATE_MOUSEDOWN._HAND_DRAW_JOINPATHS_P1:
                    //pathTemp = new CurveShape(map, new Point(0, 0), new Point(0, 0), false);
                    if (mouseWasDownOn != null)
                    {
                        string elementName = mouseWasDownOn.Name;
                        string type = (e.Source.GetType().Name);
                        if ((elementName != "") && ((type == "StraightPath") || (elementName.Split('x')[0] == "StraightPath")))
                        {
                            //draw_StartPoint = list_Path[elementName].props.cornerPoints[4];
                            Global_Mouse.ctrl_MouseDown = Global_Mouse.STATE_MOUSEDOWN._HAND_DRAW_JOINPATHS_FINISH;
                            Global_Mouse.ctrl_MouseMove = Global_Mouse.STATE_MOUSEMOVE._HAND_DRAW_JOINPATHS;
                        }
                    }
                    break;
                case Global_Mouse.STATE_MOUSEDOWN._HAND_DRAW_STRAIGHT_FINISH:
                    if (mouseWasDownOn != null)
                    {
                        string elementName = mouseWasDownOn.Name;
                        Global_Mouse.ctrl_MouseDown = Global_Mouse.STATE_MOUSEDOWN._HAND_DRAW_STRAIGHT_P1;
                        Global_Mouse.ctrl_MouseMove = Global_Mouse.STATE_MOUSEMOVE._NORMAL; //stop draw
                        //pathTemp.props._oriMousePos = pathTemp.props.cornerPoints[0];
                        //pathTemp.props._desMousePos = pathTemp.props.cornerPoints[4];
                        //list_Path.Add(pathTemp.Name, pathTemp);
                    }
                    break;
                case Global_Mouse.STATE_MOUSEDOWN._HAND_DRAW_CURVEUP_FINISH:
                    if (mouseWasDownOn != null)
                    {
                        string elementName = mouseWasDownOn.Name;
                        Global_Mouse.ctrl_MouseDown = Global_Mouse.STATE_MOUSEDOWN._HAND_DRAW_CURVEUP_P1;
                        Global_Mouse.ctrl_MouseMove = Global_Mouse.STATE_MOUSEMOVE._NORMAL; //stop draw
                        //pathTemp.props._oriMousePos = pathTemp.props.cornerPoints[7];
                        //pathTemp.props._desMousePos = pathTemp.props.cornerPoints[5];
                        //list_Path.Add(pathTemp.Name, pathTemp);
                    }
                    break;
                case Global_Mouse.STATE_MOUSEDOWN._HAND_DRAW_CURVEDOWN_FINISH:
                    if (mouseWasDownOn != null)
                    {
                        string elementName = mouseWasDownOn.Name;
                        Global_Mouse.ctrl_MouseDown = Global_Mouse.STATE_MOUSEDOWN._HAND_DRAW_CURVEDOWN_P1;
                        Global_Mouse.ctrl_MouseMove = Global_Mouse.STATE_MOUSEMOVE._NORMAL; //stop draw
                        //pathTemp.props._oriMousePos = pathTemp.props.cornerPoints[7];
                        //pathTemp.props._desMousePos = pathTemp.props.cornerPoints[5];
                        //list_Path.Add(pathTemp.Name, pathTemp);
                    }
                    break;
                case Global_Mouse.STATE_MOUSEDOWN._HAND_DRAW_JOINPATHS_FINISH:
                    if (mouseWasDownOn != null)
                    {
                        string elementName = mouseWasDownOn.Name;
                        string type = (e.Source.GetType().Name);
                        if ((elementName != "") && ((type == "StraightPath") || (elementName.Split('x')[0] == "StraightPath")))
                        {
                            Global_Mouse.ctrl_MouseDown = Global_Mouse.STATE_MOUSEDOWN._HAND_DRAW_JOINPATHS_P1;
                            Global_Mouse.ctrl_MouseMove = Global_Mouse.STATE_MOUSEMOVE._NORMAL; //stop draw
                            //pathTemp.props._oriMousePos = pathTemp.props.cornerPoints[7];
                            //pathTemp.props._desMousePos = pathTemp.props.cornerPoints[5];
                            //list_Path.Add(pathTemp.Name, pathTemp);
                        }
                        else
                        {
                            MessageBox.Show("JoinPaths is only accept between two StraightPath! \n And only link Tail-Head");
                        }
                    }
                    break;
                default:
                    {
                        break;
                    }
            }
        }

        public void PathRemove(string name)
        {
            //if (list_Path.ContainsKey(name))
            //{
            //    list_Path.Remove(name);
            //    Console.WriteLine("Remove: " + selectedItemName + "-Count: " + list_Path.Count);
            //}
        }

        public void StationRemove(string name)
        {
            if (list_Station.ContainsKey(name))
            {
                list_Station.Remove(name);
                Console.WriteLine("Remove: " + selectedItemName + "-Count: " + list_Station.Count);
            }
        }

        public void ReloadAllStation()
        {
            for (int i = 0; i < list_Station.Count; i++)
            {
                Console.WriteLine(i);
                StationShape temp = list_Station.ElementAt(i).Value;
                Console.WriteLine("Remove: " + list_Station.ElementAt(i).Key);
                temp.Remove();
            }
            list_Station.Clear();

            HttpWebRequest request = (HttpWebRequest)WebRequest.Create(Global_Object.url + "buffer/getListBuffer");
            request.Method = "GET";
            request.ContentType = @"application/json";
            HttpWebResponse response = request.GetResponse() as HttpWebResponse;
            using (Stream responseStream = response.GetResponseStream())
            {
                StreamReader reader = new StreamReader(responseStream, Encoding.UTF8);
                string result = reader.ReadToEnd();

                DataTable buffers = JsonConvert.DeserializeObject<DataTable>(result);

                foreach (DataRow dr in buffers.Rows)
                {
                    dtBuffer tempBuffer = new dtBuffer
                    {
                        creUsrId = int.Parse(dr["creUsrId"].ToString()),
                        creDt = dr["creDt"].ToString(),
                        updUsrId = int.Parse(dr["updUsrId"].ToString()),
                        updDt = dr["updDt"].ToString(),

                        bufferId = int.Parse(dr["bufferId"].ToString()),
                        bufferName = dr["bufferName"].ToString(),
                        bufferNameOld = dr["bufferNameOld"].ToString(),
                        bufferCheckIn = dr["bufferCheckIn"].ToString(),
                        bufferData = dr["bufferData"].ToString(),
                        maxBay = int.Parse(dr["maxBay"].ToString()),
                        maxBayOld = int.Parse(dr["maxBayOld"].ToString()),
                        maxRow = int.Parse(dr["maxRow"].ToString()),
                        maxRowOld = int.Parse(dr["maxRowOld"].ToString()),
                        bufferReturn = bool.Parse(dr["bufferReturn"].ToString()),
                        bufferReturnOld = bool.Parse(dr["bufferReturnOld"].ToString()),
                        //pallets
                    };
                    if (!list_Station.ContainsKey(tempBuffer.bufferId.ToString()))
                    {
                        StationShape tempStation = new StationShape(map, tempBuffer);
                        tempStation.ReDraw();
                        //tempStation.RemoveHandle += StationRemove;
                        list_Station.Add(tempStation.props.bufferDb.bufferName.ToString().Trim(), tempStation);
                        //list_Station.Add(tempStation.props.bufferDb.bufferName.ToString().Trim(), tempStation);
                    }

                }
            }
        }

        void Statectrl_MouseMove(MouseEventArgs e)
        {
            Point mousePos = e.GetPosition(map);
            var mouseWasDownOn = e.Source as FrameworkElement;


            switch (Global_Mouse.ctrl_MouseMove)
            {
                case Global_Mouse.STATE_MOUSEMOVE._NORMAL:
                    {
                        break;
                    }
                case Global_Mouse.STATE_MOUSEMOVE._MOVE_STATION:
                    {
                        //StationShape x = new StationShape();
                        //x = (StationShape)map.Children[0];
                        //x.Move(pp.X, pp.Y);
                        break;
                    }
                case Global_Mouse.STATE_MOUSEMOVE._HAND_DRAW_STRAIGHT:
                    {
                        if (Global_Mouse.ctrl_MouseDown == Global_Mouse.STATE_MOUSEDOWN._HAND_DRAW_STRAIGHT_FINISH)
                        {
                            //pathTemp.ReDraw(draw_StartPoint, mousePos);
                        }
                        break;
                    }
                case Global_Mouse.STATE_MOUSEMOVE._HAND_DRAW_CURVE:
                    {
                        if ((Global_Mouse.ctrl_MouseDown == Global_Mouse.STATE_MOUSEDOWN._HAND_DRAW_CURVEUP_FINISH) ||
                            (Global_Mouse.ctrl_MouseDown == Global_Mouse.STATE_MOUSEDOWN._HAND_DRAW_CURVEDOWN_FINISH))
                        {
                            //pathTemp.ReDraw(draw_StartPoint, mousePos);
                        }
                        break;
                    }
                case Global_Mouse.STATE_MOUSEMOVE._HAND_DRAW_JOINPATHS:
                    {
                        //if (Global_Mouse.ctrl_MouseDown == Global_Mouse.STATE_MOUSEDOWN._HAND_DRAW_JOINPATHS_FINISH)
                        //{
                        //    string elementName = mouseWasDownOn.Name;
                        //    string type = (e.Source.GetType().Name);
                        //    if ((elementName != "") && ((type == "StraightPath") || (elementName.Split('x')[0] == "StraightPath")))
                        //    {
                        //        pathTemp.ReDraw(draw_StartPoint, list_Path[elementName].props.cornerPoints[0]);
                        //    }
                        //    else if (elementName != pathTemp.Name)
                        //    {
                        //        pathTemp.ReDraw(draw_StartPoint, draw_StartPoint);
                        //    }
                        //}
                        break;
                    }
                default:
                    {
                        break;
                    }
            }
        }

        public void RedrawAllStation()
        {
            for (int i = 0; i < list_Station.Count; i++)
            {
                list_Station.ElementAt(i).Value.ReDraw();
            }
        }

        public void RedrawAllRobot()
        {
            for (int i = 0; i < list_Robot.Count; i++)
            {
                list_Robot.ElementAt(i).Value.DrawCircle();
                //Thread.Sleep(500);
            }
        }

    }
}

