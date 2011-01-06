﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Practices.Prism.ViewModel;
using Microsoft.Practices.Unity;
using Microsoft.Practices.Prism.Events;
using SketchModeller.Infrastructure.Events;
using SketchModeller.Infrastructure.Services;
using System.Threading.Tasks;
using SketchModeller.Infrastructure.Data;
using SketchModeller.Infrastructure.Shared;
using System.ComponentModel;
using Utils;

namespace SketchModeller.Modelling.Views
{
    public class SketchImageViewModel : NotificationObject, System.Windows.IWeakEventListener
    {
        private DisplayOptions displayOptions;
        private SessionData sessionData;
        private UiState uiState;

        public SketchImageViewModel()
        {
        }

        [InjectionConstructor]
        public SketchImageViewModel(DisplayOptions displayOptions, SessionData sessionData, UiState uiState)
            : this()
        {
            this.displayOptions = displayOptions;
            this.sessionData = sessionData;
            this.uiState = uiState;

            SetupDisplayOptionsSync();
            SetupSessionDataSync();
        }

        #region Points property

        private Point[] points;

        public Point[] Points
        {
            get { return points; }
            private set
            {
                points = value;
                RaisePropertyChanged(() => Points);
            }
        }

        #endregion

        #region Polylines property

        private Polyline[] polylines;

        public Polyline[] Polylines
        {
            get { return polylines; }
            set
            {
                polylines = value;
                RaisePropertyChanged(() => Polylines);
            }
        }

        #endregion

        #region Polygons property

        private Polygon[] polygons;

        public Polygon[] Polygons
        {
            get { return polygons; }
            set
            {
                polygons = value;
                RaisePropertyChanged(() => Polygons);
            }
        }

        #endregion

        #region IsSketchShown property

        private bool isSketchShown;

        public bool IsSketchShown
        {
            get { return isSketchShown; }
            private set
            {
                isSketchShown = value;
                RaisePropertyChanged(() => IsSketchShown);
            }
        }

        #endregion

        #region property sync related

        private void SetupDisplayOptionsSync()
        {
            displayOptions.AddListener(this, () => displayOptions.IsSketchShown);

            IsSketchShown = displayOptions.IsSketchShown;
        }

        private void SetupSessionDataSync()
        {
            sessionData.AddListener(this, () => sessionData.SketchData);
        }

        bool System.Windows.IWeakEventListener.ReceiveWeakEvent(Type managerType, object sender, EventArgs e)
        {
            if (managerType != typeof(PropertyChangedEventManager))
                return false;

            var eventArgs = (PropertyChangedEventArgs)e;

            if (sender == displayOptions)
            {
                if (eventArgs.Match(() => displayOptions.IsSketchShown))
                    IsSketchShown = displayOptions.IsSketchShown;
            }
            else if (sender == sessionData)
            {
                if (eventArgs.Match(() => sessionData.SketchData))
                {
                    Points = sessionData.SketchData.Points;
                    Polylines = sessionData.SketchData.Polylines;
                    Polygons = sessionData.SketchData.Polygons;
                }
            }

            return true;
        }
        
        #endregion
    }
}
