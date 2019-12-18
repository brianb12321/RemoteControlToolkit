﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Activities;
using System.ComponentModel;
using RemoteControlToolkitCore.Common.ApplicationSystem;
using RemoteControlToolkitCore.Subsystem.Workflow.ActivityDesigners;

namespace RemoteControlToolkitCore.Subsystem.Workflow.Activities
{
    [Designer(typeof(TextActivityDesigner))]
    public sealed class OutWriteLineActivity : CodeActivity
    {
        // Define an activity input argument of type string
        public InArgument<string> Text { get; set; }

        // If your activity returns a value, derive from CodeActivity<TResult>
        // and return the value from the Execute method.
        protected override void Execute(CodeActivityContext context)
        {
            // Obtain the runtime value of the Text input argument
            string text = context.GetValue(this.Text);
            RCTProcess currentProc = context.GetExtension<RCTProcess>();
            currentProc.Out.WriteLine(text);
        }
    }
}