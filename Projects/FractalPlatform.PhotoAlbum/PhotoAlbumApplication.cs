using FractalPlatform.Client.App;
using FractalPlatform.Client.UI;
using FractalPlatform.Client.UI.DOM;

namespace FractalPlatform.PhotoAlbum
{
    public class PhotoAlbumApplication : BaseApplication
    {
        public override void OnStart() => 
            CreateNewDocForArray("NewPhoto", "Photos", "{'Photos':[$]}")
                .OpenForm(result => FirstDocOf("Photos").OpenForm());
        
        public override BaseRenderForm CreateRenderForm(DOMForm form) => new RenderForm(this, form);
    }
}
