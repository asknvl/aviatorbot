﻿using Avalonia.Controls;
using aviatorbot.Model.bot;
using aviatorbot.Models.storage;
using aviatorbot.Operators;
using Microsoft.CodeAnalysis;
using ReactiveUI;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace aviatorbot.ViewModels
{
    public class operatorsVM : LifeCycleViewModelBase
    {
        #region vars
        IOperatorStorage operatorStorage;
        #endregion

        #region properties
        public ObservableCollection<BotOperators> BotOperators { get; set; } = new();

        BotOperators selectedBotOperator;
        public BotOperators SelectedBotOperator
        {
            get => selectedBotOperator;
            set
            {
                this.RaiseAndSetIfChanged(ref selectedBotOperator, value);
                //if (selectedBotOperator != null)
                //    SubContent = SelectedBotOperator;
            }
        }

        object subContent;
        public object SubContent
        {
            get => subContent;
            set
            {
                this.RaiseAndSetIfChanged(ref subContent, value);
                if (subContent is SubContentVM)
                {
                    ((SubContentVM)subContent).OnCloseRequest += () =>
                    {
                        SubContent = null;
                    };
                }
            }
        }
        #endregion



        public operatorsVM(IOperatorStorage storage)
        {
            operatorStorage = storage;
            Update();
            operatorStorage.UpdatedEvent += () => {
                Update();
            };
        }

        public void Update()
        {
            BotOperators.Clear();
            var botOperators = operatorStorage.GetAll();
            foreach (var item in botOperators)
            {
                item.OperatosParameterRequest -= Item_OperatosParameterRequest;
                item.OperatosParameterRequest += Item_OperatosParameterRequest;

                item.OperatorSelectedEvent -= Item_OperatorSelectedEvent;
                item.OperatorSelectedEvent += Item_OperatorSelectedEvent;

                BotOperators.Add(item);
            }
        }

        private void Item_OperatorSelectedEvent(BotOperators bo, Operator op)
        {
            foreach (var item in BotOperators)
            {
                if (!item.Equals(bo))
                {
                    System.Diagnostics.Debug.WriteLine(op?.letters);
                    item.SelectedOperator = null;
                }
            }

            var opVM = new operatorVM(bo, op);
            opVM.OperatorUpdatedEvent += (bo, op) => {
                operatorStorage.Add(bo.geotag, op);
                bo.SelectedOperator = null;
                SubContent = null;
            };

            SubContent = null;
            SubContent = opVM;

        }

        private void Item_OperatosParameterRequest(BotOperators bo, Operator op)
        {
            var opVM = new operatorVM(bo, op);
            opVM.OperatorUpdatedEvent += (bo, op) => {
                operatorStorage.Add(bo.geotag, op);
                Update();                
            };
       
            SubContent = opVM;

            //OperatorParametersRequest?.Invoke(op);
        }

        #region events
        public event Action<Operator> OperatorParametersRequest;
        #endregion
    }
}