﻿using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using HCSAnalyzer.Simulator.Classes;
using HCSAnalyzer.Forms;
using HCSAnalyzer.Classes._3D;
using HCSAnalyzer.Classes;
using HCSAnalyzer.Forms.IO;
using ImageAnalysis;
using HCSAnalyzer.Forms.FormsForOptions.ClassForOptions.Children;
using HCSAnalyzer.Simulator.Forms.Panels;
using HCSAnalyzer.Simulator.Forms.NewCellType;
using MIConvexHull;
using Microsoft.Msagl.GraphViewerGdi;

namespace HCSAnalyzer.Simulator.Forms
{
    public partial class FormForSimuGenerator : Form
    {

        cGlobalInfo GlobalInfo;
        public cWorld NewWorld;
        public cListClusteringAlgo ListClusteringAlgo;
        public PanelForParamCellPopulations MyPanelForParamCellPopulations;
        PanelForParamCellTypes MyPanelForParamCellTypes;


        public cListCellType ListCellTypes { get; private set; }

        public cParamAlgo GetSelectedAlgoAndParameters()
        {
            cParamAlgo ToReturn = ListClusteringAlgo.GetListParams((string)treeViewForOptions.SelectedNode.Tag);
            return ToReturn;
        }

        public FormForSimuGenerator(cGlobalInfo GlobalInfo)
        {
            Random rnd = new Random();

            // generate some random points
            Func<double> nextRandom = () => 2 * 10 * rnd.NextDouble() - 10;
            var vertices = Enumerable.Range(0, 300)
                .Select(_ => new cPoint3D(nextRandom(), nextRandom(), nextRandom()))
                .ToList();

            //var vertices = new List<Vertex>();
            //int d = 4;
            //double cs = 10.0;
            //for (int i = 0; i < d; i++)
            //{
            //    for (int j = 0; j < d; j++)
            //    {
            //        for (int k = 0; k < d; k++)
            //        {
            //            //vertices.Add(new Vertex(10 * i - 20 + rnd.NextDouble(), 10 * j - 20 - rnd.NextDouble(), 10 * k - 20 + rnd.NextDouble()));
            //            vertices.Add(new Vertex(-10 * i, 10 * j, 10 * k));
            //        }
            //    }
            //}

            // calculate the triangulation
            //  var tetrahedrons = Triangulation.CreateDelaunay<Vertex, Tetrahedron>(vertices).Cells;
            //  var tetrahedrons = VoronoiMesh.Create<cPoint3D, Tetrahedron>(vertices).Vertices;

            #region initialise cell types
            ListCellTypes = new cListCellType();
            for (int i = 0; i < 5; i++)
                ListCellTypes.Add(new cCellType(i, this));

            foreach (cCellType item in this.ListCellTypes)
            {
                int Idx = 0;
                foreach (cTransitionValue Transition in item.ListInitialTransitions)
                {
                    Transition.DestType = this.ListCellTypes[Idx++];
                }                                
            }

            #endregion

            this.GlobalInfo = GlobalInfo;
            InitializeComponent();
            MyPanelForParamCellPopulations = new PanelForParamCellPopulations(new cPoint3D(600, 600, 50), this);

            this.treeViewForOptions.ExpandAll();
            this.treeViewForOptions.SelectedNode = this.treeViewForOptions.Nodes[0];
            this.ListClusteringAlgo = new cListClusteringAlgo(new cPoint3D(600, 600, 50));

            cParamAlgo ClusteringAlgo = ListClusteringAlgo.GetListParams("WorldDimensions");
            cListValuesParam Parameters = ClusteringAlgo.GetListValuesParam();

            NewWorld = new cWorld(new cPoint3D((int)(Parameters.ListDoubleValues.Get("numericUpDownWorldDimensionY").Value),
                                   (Parameters.ListDoubleValues.Get("numericUpDownWorldDimensionY").Value),
                                   (Parameters.ListDoubleValues.Get("numericUpDownWorldDimensionZ").Value)), this);

            MyPanelForParamCellTypes = new PanelForParamCellTypes();
        }

        private void buttonRun_Click(object sender, EventArgs e)
        {
            RunSimu();
        }

        void RunSimu()
        {
            // int NumWellToProcess = 1;     
            List<string> ListNameSignature = new List<string>();
            Random RND = new Random();

            if(NewWorld.ListCells!=null)    NewWorld.ListCells.Clear();


            ListNameSignature.Add("PosX");
            ListNameSignature.Add("PosY");
            ListNameSignature.Add("PosZ");
            ListNameSignature.Add("Volume");
            ListNameSignature.Add("DistanceToCenter");
            ListNameSignature.Add("FullPathDistance");
            ListNameSignature.Add("CellType");

            cSQLiteDatabase SQDB = null;
            string PlateName = "SimulatedPlate";

            bool IsExportToDB = (bool)ListClusteringAlgo.GetListParams("PlateDesign").GetListValuesParam().ListCheckValues.Get("checkBoxExportToDB").Value;
            int NumCols = (int)ListClusteringAlgo.GetListParams("PlateDesign").GetListValuesParam().ListDoubleValues.Get("numericUpDownNumCols").Value;
            int NumRows = (int)ListClusteringAlgo.GetListParams("PlateDesign").GetListValuesParam().ListDoubleValues.Get("numericUpDownNumRows").Value;

            if (IsExportToDB)
            {

                FolderBrowserDialog WorkingFolderDialog = new FolderBrowserDialog();
                WorkingFolderDialog.ShowNewFolderButton = true;
                WorkingFolderDialog.Description = "Select the working directory";
                if (WorkingFolderDialog.ShowDialog() != DialogResult.OK) return;
                //NumWellToProcess = NumWells;

                SQDB = new cSQLiteDatabase(WorkingFolderDialog.SelectedPath + "\\" + PlateName, ListNameSignature, true);
            }

            // run the simulation
            FormForProgress MyProgressBar = new FormForProgress();
            int NumIterations = (int)ListClusteringAlgo.GetListParams("General").GetListValuesParam().ListDoubleValues.Get("numericUpDownRunIterations").Value;

            if (IsExportToDB)
            {
                toolStripProgressBar.Maximum = NumIterations * NumCols * NumRows;
            }
            else
            {
                toolStripProgressBar.Maximum = NumIterations;
                NumCols = 1;
                NumRows = 1;
            }


            //MyProgressBar.progressBar
            // MyProgressBar.Show();


            // cParamAlgo ClusteringAlgo = ListClusteringAlgo.GetListParams("CellPopulations");
            //  cListValuesParam Parameters = ClusteringAlgo.GetListValuesParam();

            //ListView ListViewForCellPops = null;// (ListView)Parameters.ListListViewValues.Get("listViewForCellPopulations").Value;

            bool IsMemoryOn = (bool)ListClusteringAlgo.GetListParams("General").GetListValuesParam().ListCheckValues.Get("checkBoxMemory").Value;

            for (int IdxCol = 0; IdxCol < NumCols; IdxCol++)
                for (int IdxRow = 0; IdxRow < NumRows; IdxRow++)
                {
                    cWellForDatabase WellForDB = new cWellForDatabase(PlateName, IdxCol + 1, IdxRow + 1);
                    List<List<double>> ListData = new List<List<double>>();

                    cParamAlgo ClusteringAlgo = ListClusteringAlgo.GetListParams("WorldDimensions");
                    cListValuesParam Parameters = ClusteringAlgo.GetListValuesParam();

                    NewWorld.ListCells.Clear();
                    
                    NewWorld = new cWorld(new cPoint3D((int)(Parameters.ListDoubleValues.Get("numericUpDownWorldDimensionX").Value),
                                           (Parameters.ListDoubleValues.Get("numericUpDownWorldDimensionY").Value),
                                           (Parameters.ListDoubleValues.Get("numericUpDownWorldDimensionZ").Value)), this);
                    

                    //    // define a cell cycle
                    //    cCycle ClassicCellCycle = new cCycle(); // default cell cycle

                    //    Random RndForUnsychronizedPopulation = new Random();

                    //    int CellType = comboBoxCellType.SelectedIndex;
                    //    if (SQDB != null) CellType = IdxWell % comboBoxCellType.Items.Count;

                    //    // generate initiale cell population
                    //    for (int i = 0; i < (int)numericUpDownInitialCellNumber.Value; i++)
                    //    {
                    //        cCell NewCell = new cCell(new cPoint3D(NewWorld.Dimensions.X / 2, NewWorld.Dimensions.Y / 2, 0),
                    //                                    2,
                    //                                    CellType,                      
                    //                                    RndForUnsychronizedPopulation.NextDouble() * ClassicCellCycle.ListProba.Count);

                    //        NewWorld.ListCells.Add(NewCell);

                    foreach (ListViewItem item in MyPanelForParamCellPopulations.listViewForCellPopulations.Items)
                    {
                        if (item.Checked)
                        {
                            cCellPopulation CurrentCellPop = (cCellPopulation)item.Tag;

                            //cClassForVariable MyVar = CurrentCellPop.AssociatedVariables.FindVariable("v_CellNumber");
                            //if (MyVar.IsConstant == false)
                            //{
                            //    double NewValue = MyVar.Cst_Value + IdxWell * MyVar.Increment;
                            //}

                            //NewWorld.ListCells.AddRange(CurrentCellPop);

                            //ListVar.Add(new cClassForVariable("v_CellNumber", 100));
                            //ListVar.Add(new cClassForVariable("v_InitPosX", 0));
                            //ListVar.Add(new cClassForVariable("v_InitPosY", 0));
                            //ListVar.Add(new cClassForVariable("v_InitPosZ", 0));
                            //ListVar.Add(new cClassForVariable("v_InitPosType", 0));

                            cClassForVariable MyVarVolType = CurrentCellPop.AssociatedVariables.FindVariable("v_InitVolType");
                            cClassForVariable MyVarVol = CurrentCellPop.AssociatedVariables.FindVariable("v_InitVol");

                            cClassForVariable MyVarPosType = CurrentCellPop.AssociatedVariables.FindVariable("v_InitPosType");
                            cClassForVariable MyVarPosX = CurrentCellPop.AssociatedVariables.FindVariable("v_InitPosX");
                            cClassForVariable MyVarPosY = CurrentCellPop.AssociatedVariables.FindVariable("v_InitPosY");
                            cClassForVariable MyVarPosZ = CurrentCellPop.AssociatedVariables.FindVariable("v_InitPosZ");


                            double InitVolume = 0;
                            if (MyVarVolType.Cst_Value == 1)    // Random
                            {
                                InitVolume = (MyVarVolType.RandomInfo.Max - MyVarVolType.RandomInfo.Min) * RND.NextDouble() + MyVarVolType.RandomInfo.Min;
                            }
                            else // fixed
                            {
                                InitVolume = MyVarVol.Cst_Value;
                            }

                            foreach (cCell CurrentCell in CurrentCellPop)
                            {

                                double Volume = 0;
                                if (MyVarVol.IsConstant == false)   // random
                                {
                                    // a changer ... au lieu de min max, mettre juste standard dev.
                                    Volume = (MyVarVol.RandomInfo.Max - MyVarVol.RandomInfo.Min) * RND.NextDouble() + MyVarVol.RandomInfo.Min;
                                }
                                else
                                {
                                    if (MyVarVol.IsVariableAlongColumns)
                                        Volume = InitVolume + IdxCol * InitVolume;
                                    else if (MyVarVol.IsVariableAlongRows)
                                        Volume = InitVolume + IdxRow * InitVolume;
                                    else
                                        Volume = InitVolume;


                                }
                                CurrentCell.InitialVolume = Volume;


                                if(MyVarPosType.Cst_Value==0) // center
                                {
                                    cPoint3D CellPos = new cPoint3D(NewWorld.Dimensions.X / 2.0, NewWorld.Dimensions.Y / 2.0, NewWorld.Dimensions.Z / 2.0);
                                    CurrentCell.CentroidPosition = CellPos;
                                }
                                else if (MyVarPosType.Cst_Value==1) // random
                                {
                                    cPoint3D CellPos = new cPoint3D(NewWorld.Dimensions.X * RND.NextDouble(),
                                                                    NewWorld.Dimensions.Y * RND.NextDouble(), 
                                                                    NewWorld.Dimensions.Z * RND.NextDouble());
                                    CurrentCell.CentroidPosition = CellPos;
                                }
                                else if (MyVarPosType.Cst_Value==2) // fixed
                                {
                                    double X_start;

                                    if (MyVarPosX.IsVariableAlongColumns)
                                        X_start = MyVarPosX.Cst_Value + IdxCol * MyVarPosX.Increment;
                                    else if (MyVarPosX.IsVariableAlongRows)
                                        X_start = MyVarPosX.Cst_Value + IdxRow * MyVarPosX.Increment;
                                    else
                                        X_start = MyVarPosX.Cst_Value;

                                    double Y_start;
                                    if (MyVarPosY.IsVariableAlongColumns)
                                        Y_start = MyVarPosY.Cst_Value + IdxCol * MyVarPosY.Increment;
                                    else if (MyVarPosY.IsVariableAlongRows)
                                        Y_start = MyVarPosY.Cst_Value + IdxRow * MyVarPosY.Increment;
                                    else
                                        Y_start = MyVarPosY.Cst_Value;

                                    double Z_start;
                                    if (MyVarPosZ.IsVariableAlongColumns)
                                        Z_start = MyVarPosZ.Cst_Value + IdxCol * MyVarPosZ.Increment;
                                    else if (MyVarPosZ.IsVariableAlongRows)
                                        Z_start = MyVarPosZ.Cst_Value + IdxRow * MyVarPosZ.Increment;
                                    else
                                        Z_start = MyVarPosZ.Cst_Value;


                                    cPoint3D CellPos = new cPoint3D(X_start,
                                                                    Y_start,
                                                                    Z_start);
                                    CurrentCell.CentroidPosition = CellPos;
                                }


                                CurrentCell.MemoryOn = true;
                                CurrentCell.PreviousStates.Clear();

                                NewWorld.ListCells.Add(CurrentCell);
                            }

                        }
                    }
                    //NewWorld.ListCells.AddRange(

                    //}
                    //    //{
                    //    //    cCell NewCell = new cCell(new cPoint3D(IdxPosX, IdxPosY, NewWorld.Dimensions.Z / 2),
                    //    //        10,
                    //    //        CellType,
                    //    //        RndForUnsychronizedPopulation.NextDouble() * ClassicCellCycle.ListProba.Count);
                    //    //    NewWorld.ListCells.Add(NewCell);
                    //    //}



                    for (int IdxSimu = 0; IdxSimu < NumIterations; IdxSimu++)
                    {
                      //  toolStripProgressBar.Value = IdxSimu + NumIterations * (IdxRow * NumCols + IdxCol);
                        //MyProgressBar.label.Text = "It. " + IdxSimu;
                        //MyProgressBar.label.Text += " - " + NewWorld.ListCells.Count + " cells.";
                        //MyProgressBar.label.Refresh();
                        NewWorld.RunSimu(1);
                    }

                    toolStripStatusLabel.Text = "Current simulation: " + NumIterations + " iterations. " + NewWorld.ListCells.Count + " cells";

                    if (SQDB != null)
                    {
                        foreach (cCell TmpCell in NewWorld.ListCells)
                        {
                            List<double> Signature = new List<double>();
                            Signature.Add(TmpCell.CentroidPosition.X);
                            Signature.Add(TmpCell.CentroidPosition.Y);
                            Signature.Add(TmpCell.CentroidPosition.Z);
                            Signature.Add(TmpCell.GetVolume());
                            Signature.Add(TmpCell.CentroidPosition.DistTo(new cPoint3D(NewWorld.Dimensions.X / 2, NewWorld.Dimensions.Y / 2, NewWorld.Dimensions.Z / 2)));
                            
                            cInfoCell InfoCell = new cInfoCell(TmpCell);
                            Signature.Add(InfoCell.GetDistPath());

                            Signature.Add(this.ListCellTypes.FindIdxType(TmpCell.Type));
                            ListData.Add(Signature);
                        }
                        WellForDB.AddListSignatures(ListData);
                        SQDB.AddNewWell(WellForDB);
                    }
                }
            //MyProgressBar.Close();
            toolStripProgressBar.Value = 0;
            if (SQDB != null) SQDB.CloseConnection();
            // groupBoxForResults.Enabled = true;
        }

        private void button3DVisualization_Click(object sender, EventArgs e)
        {
            FormFor3DVisu WindowFor3D = new FormFor3DVisu(GlobalInfo.CurrentScreen, this);
            WindowFor3D.Show();
        }

        private void radioButtonEvolutionCellNumber_MouseClick(object sender, MouseEventArgs e)
        {
            if (e.Button == System.Windows.Forms.MouseButtons.Right)
            {


            }
        }

        private void button2DVisualization_Click(object sender, EventArgs e)
        {

        }

        public class cListClusteringAlgo : List<cParamAlgo>
        {
            public cListClusteringAlgo(cPoint3D WorldDims)
            {
                this.Add(new cParamWorldDimensions("WorldDimensions"));
                this.Add(new cParamGeneral("General"));
                this.Add(new cParamPlateDesign("PlateDesign"));
                this.Add(new cParamCellTypes("CellTypes"));
                this.Add(new cParam3D("3D"));
            }

            public Panel GetPanel(string Name)
            {
                if (Name == null) return null;

                foreach (var item in this)
                    if (item.Name == Name) return item.GetPanel();

                return null;
            }

            public cParamAlgo GetListParams(string CategoryName)
            {
                foreach (cParamAlgo item in this)
                {
                    if (item.Name == CategoryName) return item;
                }
                return null;

            }
        }

        [Serializable]
        public class cParamWorldDimensions : cParamAlgo
        {
            public cParamWorldDimensions(string Name)
                : base(Name)
            {
                PanelForParamWorldDimensions PanelForOption = new PanelForParamWorldDimensions();
                this.PanelToDisplay = PanelForOption.panel;
            }
        }


        [Serializable]
        public class cParamGeneral : cParamAlgo
        {
            public cParamGeneral(string Name)
                : base(Name)
            {
                PanelForParamGeneral PanelForOption = new PanelForParamGeneral();
                this.PanelToDisplay = PanelForOption.panel;
            }
        }

        [Serializable]
        public class cParamPlateDesign : cParamAlgo
        {
            public cParamPlateDesign(string Name)
                : base(Name)
            {
                PanelForParamPlateDesign PanelForOption = new PanelForParamPlateDesign();
                this.PanelToDisplay = PanelForOption.panel;
            }
        }

        [Serializable]
        public class cParamCellTypes : cParamAlgo
        {
            public cParamCellTypes(string Name)
                : base(Name)
            {
                PanelForParamCellTypes PanelForOption = new PanelForParamCellTypes();
                this.PanelToDisplay = PanelForOption.panel;
            }
        }

        [Serializable]
        public class cParam3D : cParamAlgo
        {
            public cParam3D(string Name)
                : base(Name)
            {
                PanelForParams3D PanelForOption = new PanelForParams3D();
                this.PanelToDisplay = PanelForOption.panel;
            }
        }

        private void treeViewForOptions_AfterSelect(object sender, TreeViewEventArgs e)
        {
            this.panelForDisplay.Controls.Clear();
            string TagName = (string)e.Node.Tag;

            if (TagName == "CellPopulations")
            {
                //PanelForParamCellPopulations
                this.panelForDisplay.Controls.Add(MyPanelForParamCellPopulations);
            }
            else if (TagName == "CellTypes")
            {
                //PanelForParamCellPopulations
                //MyPanelForParamCellTypes.RefreshDisplay(Parent);
                this.panelForDisplay.Controls.Add(MyPanelForParamCellTypes);
                MyPanelForParamCellTypes.RefreshDisplay(this);
            }
            else
            {
                Panel PanelToDisp = ListClusteringAlgo.GetPanel(TagName);
                if (PanelToDisp == null) return;
                this.panelForDisplay.Controls.Add(PanelToDisp);
            }
        }

        #region Display
        void Display3D()
        {
            FormFor3DVisu WindowFor3D = new FormFor3DVisu(GlobalInfo.CurrentScreen, this);
            WindowFor3D.Show();
        }

        void Display2D()
        {

            cImage TestImage = new cImage((int)NewWorld.Dimensions.X, (int)NewWorld.Dimensions.Y, 1, 1);
            TestImage.Name = "2D World";
            /*for (int IdxChannel = 0; IdxChannel < TestImage.NumChannels; IdxChannel++)
                for (int Y = 0; Y < TestImage.Height; Y++)
                    for (int X = 0; X < TestImage.Width; X++)
                    {
                        TestImage.Data[IdxChannel].Data[X + Y * TestImage.Width] = X * Y / 10;
                    }
            */
            //     cImage FilteredImage = new cImage(TestImage.Width, TestImage.Height, TestImage.Depth, TestImage.NumChannels);
            //     ImageAnalysisFiltering.cImageFilterMedian FilterMedian = new ImageAnalysisFiltering.cImageFilterMedian(TestImage, 0, FilteredImage, 0);
            //     FilterMedian.radius = 2;
            //     FilterMedian.Run();

            cImageViewer NewView = new cImageViewer();
            NewView.SetImage(TestImage);

            //cImageViewer NewView1 = new cImageViewer();
            //NewView1.SetImage(FilteredImage);

            //NewView.AddNotation(new ObjectForNotations.cString("This is a test", new Point(10, 10), Color.Red, 20));

            foreach (var item in NewWorld.ListCells)
            {
                NewView.AddNotation(new ObjectForNotations.cDisk(new Point((int)item.CentroidPosition.X, (int)item.CentroidPosition.Y),
                                       item.Type.TypeColor, (int)item.GetVolume()));
            }

            //for (int Idx = 0; Idx < 120; Idx += 10)
            //    NewView.AddNotation(new ObjectForNotations.cDisk(new Point(Idx * 10, Idx * 10), Color.FromArgb(Idx, Idx, 50), Idx));

            GlobalInfo.DisplayViewer(NewView);
        }

        private void dToolStripMenuItemDisp3D_Click(object sender, EventArgs e)
        {
            Display3D();
        }

        private void dToolStripMenuItemDisp2D_Click(object sender, EventArgs e)
        {
            Display2D();
        }

        private void dToolStripMenuItemDisplay3D_Click(object sender, EventArgs e)
        {
            Display3D();
        }

        private void dToolStripMenuItemDisplay2D_Click(object sender, EventArgs e)
        {
            Display2D();
        }

        #endregion

        private void runToolStripMenuItem_Click(object sender, EventArgs e)
        {
            RunSimu();
        }

        private void newCellTypeToolStripMenuItem_Click(object sender, EventArgs e)
        {
            CreateNewCellType();
        }

        public void CreateNewCellType()
        {
            FormForNewCellType WindowForNewCellType = new FormForNewCellType(this);
            if (WindowForNewCellType.ShowDialog() != System.Windows.Forms.DialogResult.OK) return;

            cCellType NewCellType = WindowForNewCellType.NewCellType;

            ListCellTypes.Add(NewCellType);
            MyPanelForParamCellTypes.RefreshDisplay(this);
        }

        public void EditCellType(cCellType CurrentType)
        {
            FormForNewCellType WindowForNewCellType = new FormForNewCellType(this,CurrentType );
            if (WindowForNewCellType.ShowDialog() != System.Windows.Forms.DialogResult.OK) return;

            int IdxCellType = -1;
            foreach (var item in ListCellTypes)
            {
                IdxCellType++;
                if (item.Name == CurrentType.Name)
                    break;
            }
            ListCellTypes[IdxCellType] = WindowForNewCellType.NewCellType;

            //cCellType NewCellType = WindowForNewCellType.NewCellType;
            
            
            //ListCellTypes.Find(CurrentType) = NewCellType;
            //ListCellTypes.Add(NewCellType);
            MyPanelForParamCellTypes.RefreshDisplay(this);
        }

        private void cellTypesRelationshipsToolStripMenuItem_Click(object sender, EventArgs e)
        {    
            FormForCellTypeRelationships WindowForDisplay = new FormForCellTypeRelationships(this);
            //GViewer GraphView = new GViewer();
            //Microsoft.Msagl.Drawing.Graph graph = new Microsoft.Msagl.Drawing.Graph("graph");
            //GraphView.Size = new System.Drawing.Size(WindowForDisplay.panel.Width, WindowForDisplay.panel.Height);
            //GraphView.Anchor = (AnchorStyles.Bottom | AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right);




            //GraphView.Graph = graph;


            //WindowForDisplay.panel.Controls.Add(GraphView);
            WindowForDisplay.Show();

            /*
            GraphView.Size = new System.Drawing.Size(PanelForVisualFeedback.Width, PanelForVisualFeedback.Height);
            GraphView.Anchor = (AnchorStyles.Bottom | AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right);
            PanelForVisualFeedback.Controls.Clear();
            PanelForVisualFeedback.Controls.Add(GraphView);

            FormForClassificationTree WindowForTree = new FormForClassificationTree();
            string StringForTree = J48Model.graph().Remove(0, J48Model.graph().IndexOf("{") + 2);
            WindowForTree.gViewerForTreeClassif.Graph = GlobalInfo.WindowHCSAnalyzer.ComputeAndDisplayGraph(StringForTree.Remove(StringForTree.Length - 3, 3));
            return WindowForTree;*/

        }

        private void collapseToolStripMenuItem_Click(object sender, EventArgs e)
        {
            this.treeViewForOptions.CollapseAll();
        }

        private void expandAllToolStripMenuItem_Click(object sender, EventArgs e)
        {
            this.treeViewForOptions.ExpandAll();
        }

    }
}
