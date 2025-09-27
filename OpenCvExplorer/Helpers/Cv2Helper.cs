using OpenCvSharp;

namespace OpenCvExplorer.Helpers;

public class Cv2Helper
{
    static public double CalcSharpness(Mat mat)
    {
        double sharpness = 0.0;
        if (mat == null)
            return sharpness;

        using var gray = new Mat();
        if (mat.Channels() == 3)
            Cv2.CvtColor(mat, gray, ColorConversionCodes.BGR2GRAY);
        else
            mat.CopyTo(gray);

        for (int row = 1; row < mat.Rows - 1; row++)
        {
            for (int col = 1; col < mat.Cols - 1; col++)
            {
                int dx = gray.At<byte>(row, col) * 2 - gray.At<byte>(row, col + 1) - gray.At<byte>(row, col - 1);
                int dy = gray.At<byte>(row, col) * 2 - gray.At<byte>(row + 1, col) - gray.At<byte>(row - 1, col);
                sharpness += Math.Abs(dx) * Math.Abs(dy);
            }
        }
        return sharpness;
    }
}
