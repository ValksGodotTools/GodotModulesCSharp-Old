using Godot;

namespace GodotModules 
{
    public static class InputExtensions 
    {
        private static Dictionary<ulong, string> _prevTexts = new Dictionary<ulong, string>();

        public static void Filter(this LineEdit lineEdit, Func<string, bool> filter) 
        {
            var text = lineEdit.Text;

            if (string.IsNullOrWhiteSpace(text))
                return;

            if (!filter(text)) 
            {
                lineEdit.Text = _prevTexts[lineEdit.GetInstanceId()];
                lineEdit.CaretPosition = text.Length;
                return;
            }

            _prevTexts[lineEdit.GetInstanceId()] = text;
        }
    }
}