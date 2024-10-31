
using Avalonia;
using FreqCat.ViewModels;
using Serilog;

namespace FreqCat.Commands
{
    public class LineEditCommand : ICommand
    {
        Points undoMem;
        Points redoMem;

        readonly MainViewModel viewModel;
        public LineEditCommand(MainViewModel viewModel)
        {
            Log.Debug("LineEditCommand created");
            this.viewModel = viewModel;
            undoMem = viewModel.CurrentFrqPlotPoints;
        }
        string PrintPoints(Points points)
        {
            string str = "";
            foreach (var point in points)
            {
                str += $"({point.X}, {point.Y}) ";
            }
            return str;
        }

        bool isFirstExec = true;
        public void Execute(bool isRedoing)
        {
            if (isRedoing)
            {
                Log.Debug($"Redoing LineEditCommand - redomem: {PrintPoints(redoMem)}");
                viewModel.CurrentFrqPlotPoints = redoMem;
            }
            else
            {
                Log.Debug($"Executing LineEditCommand - undomem: {PrintPoints(viewModel.CurrentFrqPlotPoints)}");
                if (!isFirstExec)
                {
                    undoMem = viewModel.CurrentFrqPlotPoints;
                }
                isFirstExec = false;
            }
        }

        public void UnExecute()
        {
            
            redoMem = viewModel.CurrentFrqPlotPoints;
            viewModel.CurrentFrqPlotPoints = undoMem;
            Log.Debug($"Unexecuting LineEditCommand - undomem: {PrintPoints(undoMem)}");
            Log.Debug($"Unexecuting LineEditCommand - redomem: {PrintPoints(redoMem)}");
        }
    }
}
