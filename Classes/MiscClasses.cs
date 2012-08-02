﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Collections;
//using Emgu.CV;
//using Emgu.CV.Structure;
//using Emgu.CV.CvEnum;
using System.Runtime.InteropServices;
using Emgu.CV;
using Emgu.CV.CvEnum;

namespace HCSAnalyzer.Classes
{




    public class cInfoClassif
    {
        public String StringForTree;
        public String StringForQuality;
        public String ConfusionMatrix;

    }


    public class cInfoClass
    {
        public int[] CorrespondanceTable;
        public List<int> ListBackAssociation = new List<int>();
        public int NumberOfClass = 0;

    }


    public class cScoreAndClass
    {
        public double Score;
        public int Class;

        public cScoreAndClass(int Class, double Score)
        {
            this.Class = Class;
            this.Score = Score;
        }
    }


    public class cInfoDescriptors
    {
        public int[] CorrespondanceTable;
        public List<int> ListBackAssociation = new List<int>();
    }


    public class cExtendedList : List<double>
    {

        public double Mean()
        {
            double Mean = 0;
            for (int i = 0; i < this.Count; i++)
                Mean += this[i];
            return Mean / (double)this.Count;
        }


        public double GetWeightedMean()
        {
            double ToReturn = 0;
            double Norm = this.Sum();

            for (int Idx = 0; Idx < this.Count; Idx++)
            {
                ToReturn += (Idx * this[Idx]);
            }

            return ToReturn / Norm;

        }

        public double Std()
        {
            double var = 0f, mean = this.Mean();
            foreach (float f in this) var += (f - mean) * (f - mean);
            return Math.Sqrt(var / (float)(this.Count - 1));
        }

        public cExtendedList Normalize(double Min, double Max)
        {
            cExtendedList ToReturn = new cExtendedList();

            if (Min == Max) return this;

            double Diff = Max - Min;

            foreach (var item in this)
            {
                ToReturn.Add((item - Min) / Diff);
            }


            return ToReturn;
        }


        public List<double[]> CreateHistogram(double Min, double Max, double Bin)
        {
            List<double[]> ToReturn = new List<double[]>();

            //float max = math.Max(data);
            if (this.Count == 0) return ToReturn;

            double step = (Max - Min) / Bin;

            int HistoSize = (int)((Max - Min) / step) + 1;

            double[] axeX = new double[HistoSize];
            for (int i = 0; i < HistoSize; i++)
            {
                axeX[i] = Min + i * step;
            }
            ToReturn.Add(axeX);

            double[] histogram = new double[HistoSize];
            //double RealPos = Min;

            int PosHisto;
            foreach (double f in this)
            {
                PosHisto = (int)((f - Min) / step);
                if ((PosHisto >= 0) && (PosHisto < HistoSize))
                    histogram[PosHisto]++;
            }
            ToReturn.Add(histogram);

            return ToReturn;
        }

        public List<double[]> CreateHistogram(double Bin)
        {
            List<double[]> ToReturn = new List<double[]>();


            //float max = math.Max(data);
            if (this.Count == 0) return ToReturn;
            double Max = this[0];
            double Min = this[0];

            for (int Idx = 1; Idx < this.Count; Idx++)
            {
                if (this[Idx] > Max) Max = this[Idx];
                if (this[Idx] < Min) Min = this[Idx];
            }

            double[] axeX;
            double[] histogram;

            if (Max == Min)
            {
                axeX = new double[1];
                axeX[0] = Max;
                ToReturn.Add(axeX);
                histogram = new double[1];
                histogram[0] = /*Max **/ this.Count;
                ToReturn.Add(histogram);
                return ToReturn;

            }

            double step = (Max - Min) / Bin;
          //  int HistoSize = (int)((Max - Min) / step)+1;
            
            axeX = new double[(int)Bin];

            for (int i = 0; i < (int)Bin; i++)
            {
                axeX[i] = Min + i * step;
            }
            ToReturn.Add(axeX);

            histogram = new double[(int)Bin];
            //double RealPos = Min;

            int PosHisto;
            foreach (double f in this)
            {
                PosHisto = (int)(((Bin-1)*(f - Min)) / (Max-Min));
               // if ((PosHisto >= 0) && (PosHisto < Bin))
                    histogram[PosHisto]++;
            }
            ToReturn.Add(histogram);

            return ToReturn;
        }



        //public List<cExtendedList> CreateHistogram(double Bins)
        //{
        //    List<cExtendedList> ToReturn = new List<cExtendedList>();


        //    //float max = math.Max(data);
        //    if (this.Count == 0) return ToReturn;
        //    double Max = this[0];
        //    double Min = this[0];

        //    for (int Idx = 1; Idx < this.Count; Idx++)
        //    {
        //        if (this[Idx] > Max) Max = this[Idx];
        //        if (this[Idx] < Min) Min = this[Idx];
        //    }

        //    cExtendedList axeX = new cExtendedList();
        //    cExtendedList histogram = new cExtendedList();

        //    if (Max == Min)
        //    {
                
        //        axeX.Add(Max);
        //        ToReturn.Add(axeX);
        //        histogram.Add(Max * this.Count);
        //        //histogram[0] = Max * this.Count;
        //        ToReturn.Add(histogram);
        //        return ToReturn;

        //    }

        //    double step = (Max - Min) / Bin;
        //    //  int HistoSize = (int)((Max - Min) / step)+1;

        //   // axeX = new double[(int)Bin];

        //    for (int i = 0; i < (int)Bin; i++)
        //    {
        //        axeX.Add(Min + i * step);
        //    }
        //    ToReturn.Add(axeX);

        //  //  histogram = new double[(int)Bin];
        //    //double RealPos = Min;

        //    int PosHisto;
        //    foreach (double f in this)
        //    {
        //        PosHisto = (int)(((Bin - 1) * (f - Min)) / (Max - Min));
        //        // if ((PosHisto >= 0) && (PosHisto < Bin))
        //        histogram[PosHisto]++;
        //    }
        //    ToReturn.Add(histogram);

        //    return ToReturn;
        //}


        public double Max()
        {
            double Max = double.MinValue;

            foreach (double val in this)
                if (val >= Max) Max = val;
            return Max;

        }

        public double Min()
        {
            double Min = double.MaxValue;

            foreach (double val in this)
                if (val <= Min) Min = val;
            return Min;

        }

        public double Dist_Euclidean(cExtendedList CompareTo)
        {
            double Res = 0;
            if (CompareTo.Count != this.Count) return -1;


            for (int i = 0; i < this.Count; i++)
            {
                Res += ((this[i] - CompareTo[i]) * (this[i] - CompareTo[i]));
            }


            return Math.Sqrt(Res);
        }

        public double Dist_Manhattan(cExtendedList CompareTo)
        {
            double Res = 0;
            if (CompareTo.Count != this.Count) return -1;

            for (int i = 0; i < this.Count; i++)
            {
                Res += Math.Abs(this[i] - CompareTo[i]);
            }


            return Res;
        }

        public double Dist_VectorCosine(cExtendedList CompareTo)
        {
            
            if (CompareTo.Count != this.Count) return -1;

            double Top = 0;
            double Bottom1 = 0;
            double Bottom2 = 0;

            for (int i = 0; i < this.Count; i++)
            {
                Top += this[i] * CompareTo[i];

                Bottom1 += this[i] * this[i];
                Bottom2 += CompareTo[i] * CompareTo[i];

            }

            double Bottom = Math.Sqrt(Bottom1)*Math.Sqrt(Bottom2);

            if(Bottom<=0) return -1;

            double ToReturn = 1 - (Top / Bottom);

            return ToReturn;
        }


        public double Dist_BhattacharyyaCoefficient(cExtendedList CompareTo)
        {
            double Res = 0;
            if (CompareTo.Count != this.Count) return -1;

            
            for (int i = 0; i < this.Count; i++)
            {
                Res += Math.Sqrt(this[i] * CompareTo[i]);
            }

            return Res;
        }



        public double Dist_EarthMover(cExtendedList CompareTo)
        {
            Matrix<float> Signature1 = new Matrix<float>(this.Count, 2);
            Matrix<float> Signature2 = new Matrix<float>(CompareTo.Count, 2);

            for (int Idx = 0; Idx < this.Count; Idx++)
            {
                Signature1[Idx, 0] = (float)this[Idx];
                Signature1[Idx, 1] = Idx;

                Signature2[Idx, 0] = (float)CompareTo[Idx];
                Signature2[Idx, 1] = Idx;
            }

            double ResutatEMD;
            ResutatEMD = CvInvoke.cvCalcEMD2(Signature1.Ptr, Signature2.Ptr, DIST_TYPE.CV_DIST_L1, null, IntPtr.Zero, IntPtr.Zero, IntPtr.Zero, IntPtr.Zero);

            return ResutatEMD;

        }


    }

}
