﻿using System;
using System.Collections.Generic;
using System.Linq;
using Livet;

namespace StarryEyes.Models.Tab
{
    public class ColumnModel
    {
        private readonly ObservableSynchronizedCollection<TabModel> _tabs =
            new ObservableSynchronizedCollection<TabModel>();

        private int _currentFocusTabIndex;

        public event Action OnCurrentFocusTabChanged;

        public ObservableSynchronizedCollection<TabModel> Tabs
        {
            get { return _tabs; }
        }

        public int CurrentFocusTabIndex
        {
            get { return _currentFocusTabIndex; }
            set
            {
                _currentFocusTabIndex = value;
                InputAreaModel.NotifyChangeFocusingTab(_tabs[value]);
                var handler = OnCurrentFocusTabChanged;
                if (handler != null) handler();
            }
        }

        public ColumnModel() { }
        public ColumnModel(params TabModel[] tabModels)
            : this(tabModels.AsEnumerable())
        {
        }

        public ColumnModel(IEnumerable<TabModel> tabModels)
        {
            tabModels.ForEach(_tabs.Add);
        }

        public void CreateTab(TabModel info)
        {
            _tabs.Add(info);
            CurrentFocusTabIndex = _tabs.Count - 1;
        }

        public void RemoveTab(int index)
        {
            _tabs.RemoveAt(index);
            CurrentFocusTabIndex = index < _tabs.Count - 1 ? index : index > 0 ? index - 1 : -1;
        }
    }
}
