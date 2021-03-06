﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using HCSAnalyzer.Classes._3D;
using HCSAnalyzer.Simulator.Forms;

namespace HCSAnalyzer.Simulator.Classes
{
    public class cWorld
    {
        public cPoint3D Dimensions { get; private set; }
        public cCellPopulation ListCells = new cCellPopulation("Complete Cell Population");
        public Random RND = new Random();
        //List<int> CellNumber = new List<int>();
        //List<cPoint3D> CellPosition = new List<cPoint3D>();

        FormForSimuGenerator Parent;
        public cWorld(cPoint3D Dimensions, FormForSimuGenerator Parent)
        {
            this.Dimensions = new cPoint3D(Dimensions.X, Dimensions.Y, Dimensions.Z);
            this.Parent = Parent;
        }

        public void RunSimu(int TickNumber)
        {
            //CellNumber.Clear();

            for (int Tick = 0; Tick < TickNumber; Tick++)
            {
                //CellNumber.Add(ListCells.Count);

                for(int IdxCell=0;IdxCell<ListCells.Count;IdxCell++)
                {
                //    if (ListCells[IdxCell].Type.CurrentType == eCellType.APOPTOTIC)
                //    {
                //        ListCells.Remove(ListCells[IdxCell]);
                //        IdxCell--;
                //        continue;
                //    }
                    ListCells[IdxCell].RunSingleTick(RND,this.Parent);
                }
            }
        }


    }
}
