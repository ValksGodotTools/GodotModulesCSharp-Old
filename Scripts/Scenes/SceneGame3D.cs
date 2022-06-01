namespace GodotModules
{
    public class SceneGame3D : AScene
    {
        private Managers _managers;

        public override void PreInitManagers(Managers managers)
        {
            _managers = managers;
        }

        public override void _Ready()
        {
            Notifications.AddListener(this, Event.OnKeyboardInput, (sender, args) => {
                _managers.ManagerScene.HandleEscape(async () => {
                    await _managers.ManagerScene.ChangeScene(GameScene.Menu);
                    _managers.MenuParticles.Emitting = true;
                    _managers.MenuParticles.Visible = true;
                });
            });
        }
    }
}
