﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using System.Windows.Forms;
using System.Data;
using System.Drawing;
using HCSAnalyzer.Forms;
using HCSAnalyzer;
using weka.core;
using HCSAnalyzer.Classes;
using HCSAnalyzer.Classes._3D;
using System.Data.SQLite;
using weka.classifiers;
using weka.classifiers.trees;

namespace LibPlateAnalysis
{
    public class cExtendPlateList : List<cPlate>
    {
        public cPlate GetPlate(string PlateName)
        {
            for (int Idx = 0; Idx < this.Count; Idx++)
            {
                if (PlateName == this[Idx].Name)
                    return this[Idx];
            }
            return null;
        }

        public cPlate GetPlate(int Idx)
        {
            if (Idx < 0) return null;
            if (this.Count == 0) return null;
            if (Idx >= this.Count) return null;
            return this[Idx];
        }
    }

    public class cInfoForHierarchical
    {
        public weka.core.Instances ListInstances = null;
        public List<cWell> ListIndexedWells = new List<cWell>();
        public List<double> ListMin = new List<double>();
        public List<double> ListMax = new List<double>();

        public void UpDateMinMax(cScreening CurrentScreen)
        {
            for (int iDesc = 0; iDesc < CurrentScreen.ListDescriptors.Count; iDesc++)
            {
                if (CurrentScreen.ListDescriptors[iDesc].IsActive() == false) continue;
                double MinVal = double.MaxValue;
                double MaxVal = double.MinValue;

                foreach (cWell CurrentWell in ListIndexedWells)
                {
                    double TmpValue = CurrentWell.ListDescriptors[iDesc].GetValue();
                    if (TmpValue < MinVal) MinVal = TmpValue;
                    else if (TmpValue > MaxVal) MaxVal = TmpValue;
                }

                ListMin.Add(MinVal);
                ListMax.Add(MaxVal);
            }
        }
        
        
        public void UpDateMinMax()
        {
            for (int iDesc = 0; iDesc < ListInstances.numAttributes(); iDesc++)
            {
                double MinVal = double.MaxValue;
                double MaxVal = double.MinValue;
                foreach (Instance item in ListInstances)
                {
                    double TmpValue = item.value(iDesc);
                    if (TmpValue < MinVal) MinVal = TmpValue;
                    else if (TmpValue > MaxVal) MaxVal = TmpValue;
                }
                ListMin.Add(MinVal);
                ListMax.Add(MaxVal);
            }
        }
    }


    public class cCellBasedClassification
    {
        public Classifier ClassificationModel_CellBased;
        public Evaluation evaluation;
        public J48 J48Model { get; private set; }
        public int NumClasses { get; private set; }

        public void SetJ48Tree(J48 J48Model, int NumClasses)
        {
            this.J48Model = J48Model;
            this.NumClasses = NumClasses;
        }


        public Instances CreateInstancesWithoutClass(DataTable dt)
        {
            weka.core.FastVector atts = new FastVector();
            int columnNo = 0;

            // Descriptors loop
            for (int i = 0; i < dt.Columns.Count; i++)
            {
                //if (ParentScreening.ListDescriptors[i].IsActive() == false) continue;
                atts.addElement(new weka.core.Attribute(dt.Columns[i].ColumnName));
                columnNo++;
            }
            // weka.core.FastVector attVals = new FastVector();
            Instances data1 = new Instances("MyRelation", atts, 0);

            for (int IdxRow = 0; IdxRow < dt.Rows.Count; IdxRow++)
            {
                double[] vals = new double[data1.numAttributes()];
                for (int Col = 0; Col < columnNo; Col++)
                {
                    //if (ParentScreening.ListDescriptors[Col].IsActive() == false) continue;
                    vals[Col] = double.Parse(dt.Rows[IdxRow][Col].ToString());
                }
                data1.add(new DenseInstance(1.0, vals));
            }

            return data1;
        }


        public FormForClassificationTree DisplayTree(cGlobalInfo GlobalInfo)
        {
            FormForClassificationTree WindowForTree = new FormForClassificationTree();
            if (J48Model == null) return null;
            string StringForTree = J48Model.graph().Remove(0, J48Model.graph().IndexOf("{") + 2);
            WindowForTree.gViewerForTreeClassif.Graph = GlobalInfo.WindowHCSAnalyzer.ComputeAndDisplayGraph(StringForTree.Remove(StringForTree.Length - 3, 3));
            WindowForTree.richTextBoxConsoleForClassification.Clear();
            if (evaluation != null)
            {
                WindowForTree.richTextBoxConsoleForClassification.AppendText(evaluation.toSummaryString());
                WindowForTree.richTextBoxConsoleForClassification.AppendText(evaluation.toMatrixString());
            }
            return WindowForTree;
        }


    }

    public class cScreening
    {

        public cScreening()
        {

        }

        public bool ISLoading = true;

        public cReference Reference = null;

        public cCellBasedClassification CellBasedClassification = new cCellBasedClassification();

        public c3DWorld _3DWorldForPlateDisplay;

        public void Close3DView()
        {
            if (_3DWorldForPlateDisplay != null)
            {
                _3DWorldForPlateDisplay.ren1.RemoveAllViewProps(); 
                _3DWorldForPlateDisplay.Terminate();
                _3DWorldForPlateDisplay = null;

            }

        }

        public Point ptOriginal = new Point();
        public Point ptLast = new Point();

        public Point ClientPosFirst = new Point();
        public Point ClientPosLast = new Point();

        public List<string> ListPlateBaseddescriptorNames;

        public int GetNumberOfActiveDescriptor()
        {
            int Res = 0;
            for (int i = 0; i < this.ListDescriptors.Count; i++)
                if (this.ListDescriptors[i].IsActive() == true) Res++;

            return Res;
        }

        public string Name;
        public cExtendPlateList ListPlatesActive;
        public cExtendPlateList ListPlatesAvailable;

        public int SelectedClass;
        public int Columns;
        public int Rows;
        public bool IsSelectionApplyToAllPlates = false;
        public int CurrentDisplayPlateIdx = 0;


        public cGlobalInfo GlobalInfo;
        public cListDescriptors ListDescriptors;

        public Label LabelForMin;
        public Label LabelForMax;
        public Panel PanelForLUT;
        public Panel PanelForPlate;

        /// <summary>
        /// Check if at least one descriptor is checked
        /// </summary>
        /// <returns>true if at leaset one descriptor is checked</returns>
        public bool IsSelectedDescriptors()
        {
            bool ToReturn = false;
            for (int i = 0; i < this.ListDescriptors.Count; i++)
            {
                if (this.ListDescriptors[i].IsActive()) return true;
            }
            return ToReturn;
        }

        #region Weka based clustering and classification

        /// <summary>
        /// Create an instances structure without classes for unsupervised methods
        /// </summary>
        /// <returns>a weka Instances object</returns>
        public Instances CreateInstancesWithoutClass()
        {
            weka.core.FastVector atts = new FastVector();
            int columnNo = 0;

            // Descriptors loop
            for (int i = 0; i < this.ListDescriptors.Count; i++)
            {
                if (this.ListDescriptors[i].IsActive() == false) continue;
                atts.addElement(new weka.core.Attribute(this.ListDescriptors[i].GetName()));
                columnNo++;
            }
            weka.core.FastVector attVals = new FastVector();
            Instances data1 = new Instances("MyRelation", atts, 0);

            foreach (cPlate CurrentPlate in this.ListPlatesActive)
            {
                //   foreach (cWell CurrentWell in CurrentPlate.ListActiveWells)
                //  {
                for (int j = 0; j < this.Columns; j++)
                    for (int i = 0; i < this.Rows; i++)
                    {
                        cWell CurrentWell = CurrentPlate.GetWell(j, i, false);

                        if (CurrentWell == null) continue;

                        double[] vals = new double[data1.numAttributes()];
                        int IdxRealCol = 0;

                        for (int Col = 0; Col < this.ListDescriptors.Count; Col++)
                        {
                            if (this.ListDescriptors[Col].IsActive() == false) continue;
                            vals[IdxRealCol++] = CurrentWell.ListDescriptors[Col].GetValue();
                        }
                        data1.add(new DenseInstance(1.0, vals));
                    }
                // }
            }
            return data1;
        }

        public cInfoForHierarchical CreateInstancesWithUniqueClasse()
        {
            cInfoForHierarchical InfoForHierarchical = new cInfoForHierarchical();

            weka.core.FastVector atts = new FastVector();
            int columnNo = 0;

            for (int i = 0; i < this.ListDescriptors.Count; i++)
            {
                if (ListDescriptors[i].IsActive() == false) continue;
                atts.addElement(new weka.core.Attribute(ListDescriptors[i].GetName()));
                columnNo++;
            }

            weka.core.FastVector attVals = new FastVector();
            atts.addElement(new weka.core.Attribute("Class", attVals));

            InfoForHierarchical.ListInstances = new Instances("MyRelation", atts, 0);
            int IdxWell = 0;
            foreach (cPlate CurrentPlate in this.ListPlatesActive)
            {
                foreach (cWell CurrentWell in CurrentPlate.ListActiveWells)
                {
                    if (CurrentWell.GetClassIdx() == -1) continue;
                    attVals.addElement("Class" + (IdxWell).ToString());
                    InfoForHierarchical.ListIndexedWells.Add(CurrentWell);
                    double[] vals = new double[InfoForHierarchical.ListInstances.numAttributes()];

                    int IdxCol = 0;
                    for (int Col = 0; Col < this.ListDescriptors.Count; Col++)
                    {
                        if (this.ListDescriptors[Col].IsActive() == false) continue;
                        vals[IdxCol++] = CurrentWell.ListDescriptors[Col].GetValue();
                    }
                    vals[columnNo] = IdxWell;
                    InfoForHierarchical.ListInstances.add(new DenseInstance(1.0, vals));
                    IdxWell++;
                }
            }
            InfoForHierarchical.ListInstances.setClassIndex((InfoForHierarchical.ListInstances.numAttributes() - 1));
            return InfoForHierarchical;
        }

        /// <summary>
        /// Create an instances structure with classes for supervised methods
        /// </summary>
        /// <param name="NumClass"></param>
        /// <returns></returns>
        public Instances CreateInstancesWithClasses(cInfoClass InfoClass, int NeutralClass)
        {
            weka.core.FastVector atts = new FastVector();

            int columnNo = 0;

            for (int i = 0; i < this.ListDescriptors.Count; i++)
            {
                if (this.ListDescriptors[i].IsActive() == false) continue;
                atts.addElement(new weka.core.Attribute(this.ListDescriptors[i].GetName()));
                columnNo++;
            }

            weka.core.FastVector attVals = new FastVector();

            for (int i = 0; i < InfoClass.NumberOfClass; i++)
                attVals.addElement("Class" + (i).ToString());

            atts.addElement(new weka.core.Attribute("Class", attVals));

            Instances data1 = new Instances("MyRelation", atts, 0);
            int IdxWell = 0;
            foreach (cPlate CurrentPlate in this.ListPlatesActive)
            {
                foreach (cWell CurrentWell in CurrentPlate.ListActiveWells)
                {
                    if (CurrentWell.GetClassIdx() == NeutralClass) continue;
                    double[] vals = new double[data1.numAttributes()];

                    int IdxCol = 0;
                    for (int Col = 0; Col < this.ListDescriptors.Count; Col++)
                    {
                        if (this.ListDescriptors[Col].IsActive() == false) continue;
                        vals[IdxCol++] = CurrentWell.ListDescriptors[Col].GetValue();
                    }
                    vals[columnNo] = InfoClass.CorrespondanceTable[CurrentWell.GetClassIdx()];
                    data1.add(new DenseInstance(1.0, vals));
                    IdxWell++;
                }
            }
            data1.setClassIndex((data1.numAttributes() - 1));
            return data1;
        }

        /// <summary>
        /// Assign a class to each well based on table
        /// </summary>
        /// <param name="ListClasses">Table containing the classes</param>
        public void AssignClass(double[] ListClasses)
        {
            int idxClass = 0;
            foreach (cPlate CurrentPlate in this.ListPlatesActive)
            {
                //foreach (cWell CurrentWell in CurrentPlate.ListActiveWells)
                //{

                for (int j = 0; j < this.Columns; j++)
                    for (int i = 0; i < this.Rows; i++)
                    {
                        cWell CurrentWell = CurrentPlate.GetWell(j, i, false);

                        if (CurrentWell == null) continue;


                        CurrentWell.SetClass((int)ListClasses[idxClass++]);
                    }
                CurrentPlate.UpDateWellsSelection();
            }
        }


        public void UpdateListActiveWell()
        {
            foreach (cPlate CurrentPlate in this.ListPlatesActive)
            {
                CurrentPlate.ListActiveWells.Clear();
                for (int j = 0; j < this.Rows; j++)
                    for (int i = 0; i < this.Columns; i++)
                    {
                        cWell TempWell = CurrentPlate.GetWell(i, j, false);
                        if ((TempWell == null) || (TempWell.GetClassIdx() == -1)) continue;
                        CurrentPlate.ListActiveWells.Add(TempWell);
                    }
            }
            return;
        }

        public cInfoDescriptors BuildInfoDesc()
        {
            // int[] ListClasses = UpdateNumberOfClass();

            cInfoDescriptors InfoDescriptors = new cInfoDescriptors();
            InfoDescriptors.CorrespondanceTable = new int[this.ListDescriptors.Count];
            int Idx = 0;
            for (int i = 0; i < InfoDescriptors.CorrespondanceTable.Length; i++)
            {
                if (this.ListDescriptors[i].IsActive())
                {
                    InfoDescriptors.CorrespondanceTable[i] = Idx++;
                    InfoDescriptors.ListBackAssociation.Add(i);
                }
                else
                    InfoDescriptors.CorrespondanceTable[i] = -1;
            }

            return InfoDescriptors;
        }
        #endregion

        public int GetNumberOfClasses()
        {
            int NumberOfPlates = this.ListPlatesActive.Count;
            int[] CompleteListClasses = new int[11];

            foreach (cPlate CurrentPlateToProcess in this.ListPlatesActive)
            {
                int[] ListClasses = CurrentPlateToProcess.UpdateNumberOfClass();
                for (int i = 0; i < ListClasses.Length; i++) CompleteListClasses[i] += ListClasses[i];
            }

            int NumberOfClasses = 0;
            for (int i = 1; i < CompleteListClasses.Length; i++)
                if (CompleteListClasses[i] > 0) NumberOfClasses++;
            return NumberOfClasses;
        }

        public int[] GetClassPopulation()
        {
            int[] ListClass = new int[GlobalInfo.GetNumberofDefinedWellClass()];
            foreach (cPlate CurrentPlateToProcess in this.ListPlatesActive)
            {
                foreach (cWell TmpWell in CurrentPlateToProcess.ListActiveWells)
                {
                    int Class = TmpWell.GetClassIdx();
                    if (Class >= 0)
                        ListClass[Class]++;
                    else
                    {
                    }
                }
            }


            return ListClass;
        }

        public int GetNumberOfActiveWells()
        {
            int TotalWells = 0;
            foreach (cPlate CurrentPlateToProcess in this.ListPlatesActive)
            {
                TotalWells += CurrentPlateToProcess.GetNumberOfActiveWells();
            }
            return TotalWells;
        }

        public cInfoClass GetNumberOfClassesBut(int NeutralClass)
        {
            NeutralClass++;
            int NumberOfPlates = this.ListPlatesActive.Count;
            int[] CompleteListClasses = new int[GlobalInfo.GetNumberofDefinedWellClass() + 1];

            foreach (cPlate CurrentPlateToProcess in this.ListPlatesActive)
            {
                int[] ListClasses = CurrentPlateToProcess.UpdateNumberOfClass();
                for (int i = 0; i < ListClasses.Length; i++) CompleteListClasses[i] += ListClasses[i];
            }

            cInfoClass InfoClass = new cInfoClass();
            InfoClass.CorrespondanceTable = new int[GlobalInfo.GetNumberofDefinedWellClass()];


            // int NumberOfClasses = 0;
            for (int i = 1; i < CompleteListClasses.Length; i++)
            {
                if ((CompleteListClasses[i] > 0) && (i != NeutralClass))
                {
                    InfoClass.CorrespondanceTable[i - 1] = InfoClass.NumberOfClass++;
                    InfoClass.ListBackAssociation.Add(i - 1);
                }
                else
                    InfoClass.CorrespondanceTable[i - 1] = -1;
            }
            return InfoClass;
        }

        public cScreening(string Name, cGlobalInfo GlobalInfo)
        {
            if (GlobalInfo != null)
            {

                this.GlobalInfo = GlobalInfo;
                this.ListDescriptors = new cListDescriptors(this.GlobalInfo.CheckedListBoxForDescActive, this.GlobalInfo.ComboForSelectedDesc);
            }

            //this.GlobalInfo = new cGlobalInfo(this);
            this.Name = Name;



            //ListPlate = new List<cPlate>();
            ListPlatesAvailable = new cExtendPlateList();

            //this.CurrentCompleteListBoxForPlates = CurrentCompleteListBoxForPlates;
            //this.listBoxPlateNameToProcess = listBoxPlateNameToProcess;

            // for (int i = 0; i < this.CurrentCompleteListBoxForPlates.Items.Count; i++)
            //        this.listBoxPlateNameToProcess.Items.Add(this.CurrentCompleteListBoxForPlates.Items[i]);


            // this.PanelForPlate = GlobalInfo.panelForPlate;




            this.ListPlateBaseddescriptorNames = new List<string>();
            this.ListPlateBaseddescriptorNames.Add("Row_Pos");
            this.ListPlateBaseddescriptorNames.Add("Col_Pos");
            this.ListPlateBaseddescriptorNames.Add("Dist_To_Border");
            this.ListPlateBaseddescriptorNames.Add("Dist_To_Center");
            //this.ListPlateBaseddescriptorNames.Add("Idx_X_Based");
            //this.ListPlateBaseddescriptorNames.Add("Idx_Y_Based");

        }

        public int GetNumberOfOriginalPlates()
        {
            return ListPlatesAvailable.Count;
        }

        public int GetSelectionType()
        {
            return this.SelectedClass;
        }

        public void SetSelectionType(int State)
        {
            this.SelectedClass = State;
        }

        public void AddPlate(cPlate Plate)
        {
            ListPlatesAvailable.Add(Plate);
        }

        public cPlate GetCurrentDisplayPlate()
        {
            if (ListPlatesActive == null) return null;

            return ListPlatesActive.GetPlate(CurrentDisplayPlateIdx);
        }

        /// <summary>
        /// This function tranfers all the available plates to the activated plate list
        /// </summary>
        public void UpDatePlateListWithFullAvailablePlate()
        {
            this.ListPlatesActive = new cExtendPlateList();
            foreach (cPlate Plate in ListPlatesAvailable) this.ListPlatesActive.Add(Plate);

            // this.ListPlatesAvailable = new cExtendPlateList();
            //  foreach (cPlate Plate in ListPlatesAvailable) this.ListPlatesAvailable.Add(Plate);

        }

        public cPlate GetPlateIfNameIsContainIn(string PlateName)
        {
            for (int Idx = 0; Idx < ListPlatesAvailable.Count; Idx++)
            {
                if (this.ListPlatesAvailable[Idx].Name == PlateName)
                    return ListPlatesAvailable[Idx];
            }
            return null;
        }

        public void LoadData(string Path, int col, int row)
        {
            this.Columns = col;
            this.Rows = row;

            IEnumerable<string> ListDirectories = Directory.EnumerateDirectories(Path);

            foreach (string DirectoryName in ListDirectories)
            {
                string PlateName = DirectoryName.Remove(0, DirectoryName.LastIndexOf("\\") + 1);

                cPlate CurrentPlate = new cPlate("Cpds", PlateName, this);
                CurrentPlate.LoadFromDisk(DirectoryName);
                ListPlatesAvailable.Add(CurrentPlate);

            }

            //   for (int Desc = 0; Desc < this.ListDescriptors.Count; Desc++)
            //       ListDescriptors.Add(true);

            //    this.GlobalInfo.CurrentRichTextBox.AppendText(ListPlatesAvailable.Count + " plates processed");

            UpDatePlateListWithFullAvailablePlate();

        }

        private class CsvRow : List<string>
        {
            public string LineText { get; set; }
        }

        private class CsvFileReader : StreamReader
        {
            public CsvFileReader(Stream stream)
                : base(stream)
            {
            }

            public CsvFileReader(string filename)
                : base(filename)
            {
            }

            /// <summary>
            /// Reads a row of data from a CSV file
            /// </summary>
            /// <param name="row"></param>
            /// <returns></returns>
            public bool ReadRow(CsvRow row)
            {
                //if (row.LineText == null) return false;
                row.LineText = ReadLine();
                if (String.IsNullOrEmpty(row.LineText))
                    return false;

                int pos = 0;
                int rows = 0;

                while (pos < row.LineText.Length)
                {
                    string value;

                    // Special handling for quoted field
                    if (row.LineText[pos] == '"')
                    {
                        // Skip initial quote
                        pos++;

                        // Parse quoted value
                        int start = pos;
                        while (pos < row.LineText.Length)
                        {
                            // Test for quote character
                            if (row.LineText[pos] == '"')
                            {
                                // Found one
                                pos++;

                                // If two quotes together, keep one
                                // Otherwise, indicates end of value
                                if (pos >= row.LineText.Length || row.LineText[pos] != '"')
                                {
                                    pos--;
                                    break;
                                }
                            }
                            pos++;
                        }
                        value = row.LineText.Substring(start, pos - start);
                        value = value.Replace("\"\"", "\"");
                    }
                    else
                    {
                        // Parse unquoted value
                        int start = pos;
                        while (pos < row.LineText.Length && row.LineText[pos] != ',')
                            pos++;
                        value = row.LineText.Substring(start, pos - start);
                    }

                    // Add field to list
                    if (rows < row.Count)
                        row[rows] = value;
                    else
                        row.Add(value);
                    rows++;

                    // Eat up to and including next comma
                    while (pos < row.LineText.Length && row.LineText[pos] != ',')
                        pos++;
                    if (pos < row.LineText.Length)
                        pos++;
                }
                // Delete any unused items
                while (row.Count > rows)
                    row.RemoveAt(rows);

                // Return true if any columns read
                //return (row.Count > 0);
                return true;
            }
        }

        private int[] ConvertPosition(string PosString)
        {
            int[] Pos = new int[2];

            Pos[1] = Convert.ToInt16(PosString[0]) - 64;
            Pos[0] = Convert.ToInt16(PosString.Remove(0, 1));

            return Pos;
        }

        public void ImportFromMTR(string[] FileNames, string[] SafeFileNames)
        {
            int RejectedWells = 0;
            int WellLoaded = 0;
            this.ListDescriptors.Clean();
            this.ListDescriptors.AddNew(new cDescriptorsType("Descriptor", true, 1, GlobalInfo));

            ListPlatesAvailable = new cExtendPlateList();

            for (int i = 0; i < FileNames.Length; i++)
            {
                string FileName = FileNames[i];


                StreamReader sr = new StreamReader(FileName);
                string line = sr.ReadLine();

                int Idx = line.IndexOf(" ");

                // if (Idx == -1) Idx = 0;
                string PlateNumber = line.Remove(Idx);
                int NumberOfPlate = Convert.ToInt32(PlateNumber);

                string NewLine = line.Remove(0, Idx + 1);
                line = NewLine;

                Idx = line.IndexOf(" ");
                string sRow = line.Remove(Idx);
                this.Rows = Convert.ToInt32(sRow);

                NewLine = line.Remove(0, Idx + 1);
                line = NewLine;

                this.Columns = Convert.ToInt32(line);

                for (int IdxPlate = 0; IdxPlate < NumberOfPlate; IdxPlate++)
                {
                    cPlate CurrentPlate = new cPlate("Cpds", SafeFileNames[i].Remove(SafeFileNames[i].Length - 4, 4) + "_Plate_" + IdxPlate, this);
                    this.AddPlate(CurrentPlate);
                }
                int IdxWell = 0;

                while (line != null)
                {
                    line = sr.ReadLine();
                    //IdxWell = 1;
                    int CurrentRow = IdxWell % this.Rows;
                    int CurrentCol = IdxWell / this.Rows;

                    if (line != null)
                    {
                        for (int IdxPlate = 0; IdxPlate < NumberOfPlate; IdxPlate++)
                        {
                        NEXT: ;

                            Idx = line.IndexOf("\t");
                            if (Idx != -1) NewLine = line.Remove(Idx);

                            double CurrentValue;

                            if (!double.TryParse(NewLine, out CurrentValue) || (double.IsNaN(CurrentValue)))
                            {
                                RejectedWells++;
                                goto NEXT;
                            }

                            cDescriptor Desc = new cDescriptor(CurrentValue, this.ListDescriptors[0], this);
                            cPlate CurrentPlate = ListPlatesAvailable[ListPlatesAvailable.Count - IdxPlate - 1];
                            cWell CurrentWell = new cWell(Desc, CurrentCol + 1, CurrentRow + 1, this, CurrentPlate);
                            CurrentPlate.AddWell(CurrentWell);
                            WellLoaded++;

                            NewLine = line.Remove(0, Idx + 1);
                            line = NewLine;
                        }
                    }
                    IdxWell++;
                }
                sr.Close();
            }

            UpDatePlateListWithFullAvailablePlate();

            for (int idxP = 0; idxP < this.ListPlatesActive.Count; idxP++)
                ListPlatesActive[idxP].UpDataMinMax();

            MessageBox.Show("MTR file loaded:\n" + WellLoaded + " well(s) loaded\n" + RejectedWells + " well(s) rejected.", "Process finished !", MessageBoxButtons.OK, MessageBoxIcon.Information);

            return;
        }

        public double[] ImportFromCSV(string FileName, bool IsAppend, int NumCol, int NumRow)
        {
            double[] ProcessedWell = new double[2];
            int Mode = 2;
            if (GlobalInfo.OptionsWindow.radioButtonWellPosModeSingle.Checked) Mode = 1;
            int RejectedWells = 0;
            int WellLoaded = 0;

            if (IsAppend == false)
            {
                // FormForPlateDimensions PlateDim = new FormForPlateDimensions();
                // if (PlateDim.ShowDialog() != System.Windows.Forms.DialogResult.OK)
                //     return null;
                this.Rows = NumRow;
                this.Columns = NumCol;
                this.ListDescriptors.CurrentSelectedDescriptorIdx = 0;
                //ListPlate = new List<cPlate>();
                ListPlatesAvailable = new cExtendPlateList();
            }

            string namefirst = "";

            //   for (int IdxPlate = 0; IdxPlate < FileNames.Length; IdxPlate++)
            {
                CsvFileReader CSVsr = new CsvFileReader(FileName);
                List<bool> ListIsDuplicate = new List<bool>();

                #region Initialization of the descriptors list
                namefirst = CSVsr.ReadLine();
                CSVsr.Close();
                CSVsr = new CsvFileReader(FileName);
                CsvRow ListDesc = new CsvRow();
                CSVsr.ReadRow(ListDesc);

                if (IsAppend == false)
                {
                    // this.ListDescriptorName = new List<string>();
                    this.ListDescriptors.Clean();

                    //   for (int i = 1 + Mode; i < ListDesc.Count; i++) this.ListDescriptors.Add(new cDescriptorsType(ListDesc[i],true,true);
                }
                else
                {
                    for (int i = 1 + Mode; i < ListDesc.Count; i++)
                    {
                        bool IsArleadyInList = false;
                        for (int idxDesc = 0; idxDesc < this.ListDescriptors.Count; idxDesc++)
                        {
                            if (ListDesc[i] == this.ListDescriptors[idxDesc].GetName())
                            {
                                IsArleadyInList = true;
                                break;
                            }
                        }
                        if (IsArleadyInList) ListIsDuplicate.Add(IsArleadyInList);
                    }
                    if (ListIsDuplicate.Count != this.ListDescriptors.Count) return null;
                }
                #endregion

                CsvRow CurrentDesc = new CsvRow();

                while (CSVsr.EndOfStream != true)
                {
                NEXT: ;

                    if (CSVsr.ReadRow(CurrentDesc) == false) break;

                    string PlateName = CurrentDesc[0];

                    // check if the plate exist already
                    cPlate CurrentPlate = GetPlateIfNameIsContainIn(PlateName);
                    if (CurrentPlate == null)
                    {
                        CurrentPlate = new cPlate("Cpds", PlateName, this);
                        this.AddPlate(CurrentPlate);
                    }

                    // Create the descriptor list and add it to the well, then add the well
                    List<cDescriptor> LDesc = new List<cDescriptor>();
                    for (int i = 1 + Mode; i < CurrentDesc.Count; i++)
                    {
                        cDescriptor Desc = null;
                        if (CurrentDesc[i] == null)
                        {
                            RejectedWells++;
                            goto NEXT;
                        }
                        double CurrentValue;

                        if (!double.TryParse(CurrentDesc[i], out CurrentValue) || (double.IsNaN(CurrentValue)))
                        {
                            RejectedWells++;
                            goto NEXT;
                        }

                        Desc = new cDescriptor(CurrentValue, this.ListDescriptors[i - (1 + Mode)], this);
                        LDesc.Add(Desc);
                    }
                    int[] Pos = new int[2];
                    if (Mode == 1)
                    {
                        Pos = ConvertPosition(CurrentDesc[1]);
                    }
                    else
                    {
                        Pos[0] = Convert.ToInt16(CurrentDesc[1]);
                        Pos[1] = Convert.ToInt16(CurrentDesc[2]);
                    }

                    cWell CurrentWell = new cWell(LDesc, Pos[0], Pos[1], this, CurrentPlate);
                    CurrentPlate.AddWell(CurrentWell);
                    WellLoaded++;
                }
                CSVsr.Close();
            }

            UpDatePlateListWithFullAvailablePlate();

            for (int idxP = 0; idxP < this.ListPlatesActive.Count; idxP++)
                ListPlatesActive[idxP].UpDataMinMax();

            ProcessedWell[0] = WellLoaded;
            ProcessedWell[1] = RejectedWells;

            return ProcessedWell;
        }

        public void ImportFromTXT(string[] FileNames, string[] SafeFileNames, int NumCol, int NumRow)
        {
            int RejectedWells = 0;
            int WellLoaded = 0;




            this.ListDescriptors.Clean();
            //  this.ListDescriptors.AddNew(new cDescriptorsType("Descriptor", true, 1));


            // ListPlate = new List<cPlate>();
            bool IsFirstLoop = true;


            ListPlatesAvailable = new cExtendPlateList();
            for (int IdxPlate = 0; IdxPlate < FileNames.Length; IdxPlate++)
            {
                StreamReader sr = new StreamReader(FileNames[IdxPlate]);
                string line = sr.ReadLine();

                int Idx = line.IndexOf("\t");
                string TmLine = line.Remove(0, 1);
                line = TmLine;
                // this.ListDescriptorName = new List<string>();
                // this.ListDescriptors.Clean();
                if (IsFirstLoop)
                {
                    while (Idx != -1)
                    {
                        Idx = line.IndexOf("\t");
                        if (Idx != -1)
                        {
                            string DescriptorName = line.Remove(Idx, line.Length - Idx);
                            this.ListDescriptors.AddNew(new cDescriptorsType(DescriptorName, true, 1, GlobalInfo));
                            TmLine = line.Remove(0, Idx + 1);
                            line = TmLine;
                        }
                        else if (line.Length > 0)
                        {
                            string DescriptorName = line;
                            this.ListDescriptors.AddNew(new cDescriptorsType(DescriptorName, true, 1, GlobalInfo));
                        }
                    }
                    IsFirstLoop = false;
                }
                line = sr.ReadLine();
                this.Rows = NumRow;
                this.Columns = NumCol;
                this.ListDescriptors.CurrentSelectedDescriptorIdx = 0;


                string PlateName = SafeFileNames[IdxPlate].Remove(SafeFileNames[IdxPlate].Length - 4);
                cPlate CurrentPlate = new cPlate("Cpds", PlateName, this);
                this.AddPlate(CurrentPlate);
                int IdxWell = 0;

                string NewLine;

                while (line != null)
                {
                    if (line != null)
                    {
                        int CurrentRow = Convert.ToInt16(line[0]) - 64;
                        NewLine = line.Remove(0, 1);

                        TmLine = NewLine.Remove(2);
                        int CurrentCol = Convert.ToInt16(TmLine);
                        line = NewLine.Remove(0, 3);

                        List<cDescriptor> LDesc = new List<cDescriptor>();
                        for (int i = 0; i < this.ListDescriptors.Count; i++)
                        {
                            Idx = line.IndexOf("\t");
                            if (Idx > -1) NewLine = line.Remove(Idx);

                            double CurrentValue;

                            if ((!double.TryParse(NewLine, out CurrentValue)) || (double.IsNaN(CurrentValue)))
                            {
                                RejectedWells++;
                                goto NEXT;
                            }

                            cDescriptor Desc = new cDescriptor(Convert.ToDouble(NewLine), this.ListDescriptors[i], this);
                            LDesc.Add(Desc);

                            NewLine = line.Remove(0, Idx + 1);
                            line = NewLine;
                        }
                        cWell CurrentWell = new cWell(LDesc, CurrentCol, CurrentRow, this, ListPlatesAvailable[IdxPlate]);
                        ListPlatesAvailable[IdxPlate].AddWell(CurrentWell);
                        WellLoaded++;
                    }
                NEXT:
                    IdxWell++;
                    line = sr.ReadLine();
                }
                sr.Close();


            }
            UpDatePlateListWithFullAvailablePlate();
            for (int idxP = 0; idxP < this.ListPlatesActive.Count; idxP++)
                ListPlatesActive[idxP].UpDataMinMax();
            MessageBox.Show("TXT file loaded:\n" + WellLoaded + " well(s) loaded\n" + RejectedWells + " well(s) rejected.", "Process finished !", MessageBoxButtons.OK, MessageBoxIcon.Information);




            return;
        }

        public void ApplyCurrentClassesToAllPlates()
        {
            for (int IdxPlate = 0; IdxPlate < this.ListPlatesActive.Count; IdxPlate++)
            {
                if (IdxPlate == this.CurrentDisplayPlateIdx) continue;
                cPlate TmpPlate = this.ListPlatesActive[IdxPlate];

                for (int col = 0; col < this.Columns; col++)
                    for (int row = 0; row < this.Rows; row++)
                    {
                        cWell CurrWellWithClass = ListPlatesActive[CurrentDisplayPlateIdx].GetWell(col, row, false);
                        if (CurrWellWithClass == null) continue;

                        cWell CurrWell = TmpPlate.GetWell(col, row, false);
                        if (CurrWell == null) continue;
                        int Clss = CurrWellWithClass.GetClassIdx();

                        if (Clss == -1)
                            CurrWell.SetAsNoneSelected();
                        else
                            CurrWell.SetClass(Clss);
                    }
            }


        }
    }
}
