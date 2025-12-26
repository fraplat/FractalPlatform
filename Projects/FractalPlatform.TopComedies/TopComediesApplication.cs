using FractalPlatform.Common.Clients;
using FractalPlatform.Common.Enums;
using System.Collections.Generic;
using System.Linq;
using System;
using FractalPlatform.Database.Engine.Info;
using FractalPlatform.Database.Engine;
using FractalPlatform.Client.UI.DOM;
using FractalPlatform.Client.App;
using FractalPlatform.Client.UI;

namespace FractalPlatform.TopComedies
{
    public class TopComediesApplication : BaseApplication
    {
        public override void OnStart() =>
          ModifyDocsOf("Comedies")
             .OpenForm();
    }
}
