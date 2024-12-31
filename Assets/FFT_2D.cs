using System.Drawing;
using System.Numerics;
using System.Runtime.InteropServices;
using UnityEngine;
using UnityEngine.Rendering;

public class FFT_2D : MonoBehaviour
{

    public Texture2D inputHeightmap;
    Complex[][] frequencies;
    public Texture2D outputHeightmap;
    int Size = 2048;
    public float cutoff = 0.5f;
    float lastCutoff = 0.0f;
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        Size = inputHeightmap.width;
        outputHeightmap = new Texture2D(Size, Size);
        Debug.Log("FFT_2D Start");
        frequencies = Forward(inputHeightmap);

        outputHeightmap = Inverse(frequencies);
        Debug.Log("FFT_2D End");
    }

    // Update is called once per frame
    void Update()
    {
        if (cutoff != lastCutoff)
        {
            frequencies = highPass(frequencies, cutoff);
            outputHeightmap = Inverse(frequencies);
            lastCutoff = cutoff;
        }
    }

    public Complex[][] ToComplex(Texture2D input)
    {
        int w = input.width;
        int h = input.height;

        Complex[][] result = new Complex[w][];

        for (int x = 0; x < w; x++)
        {
            result[x] = new Complex[h];
            for (int y = 0; y < h; y++)
            {
                if(input.GetPixel(x, y).a == 0)
                {
                    result[x][y] = new Complex(0, 0);
                    continue;
                }
                result[x][y] = new Complex(input.GetPixel(x,y).r, 0);
            }
        }

        return result;
    }


    public Texture2D FromComplex(Complex[][] input)
    {

        int w = input.Length;
        int h = input[0].Length;

        Texture2D result = new Texture2D(w, h);



        for (int x = 0; x < w; x++)
        {
            for (int y = 0; y < h; y++)
            {
                result.SetPixel(x, y, new UnityEngine.Color((float)input[x][y].Real, (float)input[x][y].Imaginary, 0));
            }
        }
        result.Apply();

        return result;
    }

    public Complex[] Forward(Complex[] input, bool phaseShift = true)
    {
        Complex[] result = new Complex[input.Length];
        float omega = (float)(-2.0 * Mathf.PI / input.Length);

        if (input.Length == 1)
        {
            result[0] = input[0];

            /*
            if (Complex.IsNaN(result[0]))
            {
                return new[] { new Complex(0, 0) };
            }
            */
            return result;
        }

        Complex[] evenInput = new Complex[input.Length / 2];
        Complex[] oddInput = new Complex[input.Length / 2];

        for (int i = 0; i < input.Length / 2; i++)
        {
            evenInput[i] = input[2 * i];
            oddInput[i] = input[2 * i + 1];
        }

        Complex[] even = Forward(evenInput, phaseShift);
        Complex[] odd = Forward(oddInput, phaseShift);

        for (int k = 0; k < input.Length / 2; k++)
        {
            int phase;

            if (phaseShift)
            {
                phase = k - Size / 2;
            }
            else
            {
                phase = k;
            }
            odd[k] *= Complex.FromPolarCoordinates(1, omega * phase);
        }

        for (int k = 0; k < input.Length / 2; k++)
        {
            result[k] = even[k] + odd[k];
            result[k + input.Length / 2] = even[k] - odd[k];
        }

        return result;
    }

    public Complex[][] Forward(Texture2D image)
    {
        var p = new Complex[Size][];
        var f = new Complex[Size][];
        var t = new Complex[Size][];

        var complexImage = ToComplex(image);

        for (int l = 0; l < Size; l++)
        {
            p[l] = Forward(complexImage[l]);
        }

        for (int l = 0; l < Size; l++)
        {
            t[l] = new Complex[Size];
            for (int k = 0; k < Size; k++)
            {
                t[l][k] = p[k][l];
            }
            f[l] = Forward(t[l]);
        }

        return f;
    }

    public Complex[] Inverse(Complex[] input)
    {
        for (int i = 0; i < input.Length; i++)
        {
            input[i] = Complex.Conjugate(input[i]);
        }

        var transform = Forward(input, false);

        for (int i = 0; i < input.Length; i++)
        {
            transform[i] = Complex.Conjugate(transform[i]);
        }

        return transform;
    }

    public Texture2D Inverse(Complex[][] frequencies)
    {
        var p = new Complex[Size][];
        var f = new Complex[Size][];
        var t = new Complex[Size][];

        Texture2D image = new Texture2D(Size, Size);
        
        for (int i = 0; i < Size; i++)
        {
            p[i] = Inverse(frequencies[i]);
        }

        for (int i = 0; i < Size; i++)
        {
            t[i] = new Complex[Size];
            for (int j = 0; j < Size; j++)
            {
                t[i][j] = p[j][i] / (Size * Size);
            }
            f[i] = Inverse(t[i]);
        }

        for (int y = 0; y < Size; y++)
        {
            for (int x = 0; x < Size; x++)
            {
                image.SetPixel(x,y, new UnityEngine.Color((float)f[x][y].Real, (float)f[x][y].Real, (float)f[x][y].Real));
            }
        }

        image.Apply();
        return image;
    }

    public Complex[][] highPass(Complex[][] input, float cutoff)
    {

        cutoff = Size / cutoff;

        for (int x = 0; x < Size; x++)
        {
            for (int y = 0; y < Size; y++)
            {

                Vector2Int pos = new Vector2Int(x, y);
                Vector2Int center = new Vector2Int(Size / 2, Size / 2);
                Vector2Int diff = pos - center;

                if(diff.magnitude < cutoff)
                {
                    double phase = input[x][y].Phase;// + (Mathf.PI / 4);
                    double magnitude = 0; // input[x][y].Magnitude;
                    input[x][y] = Complex.FromPolarCoordinates(magnitude, phase);
                }



                
            }
        }

        return input;
    }

    public Complex[][] lowPass(Complex[][] input, float cutoff)
    {

        cutoff = Size / cutoff;

        for (int x = 0; x < Size; x++)
        {
            for (int y = 0; y < Size; y++)
            {

                Vector2Int pos = new Vector2Int(x, y);
                Vector2Int center = new Vector2Int(Size / 2, Size / 2);
                Vector2Int diff = pos - center;

                if (diff.magnitude > cutoff)
                {
                    double phase = input[x][y].Phase;// + (Mathf.PI / 4);
                    double magnitude = 0; // input[x][y].Magnitude;
                    input[x][y] = Complex.FromPolarCoordinates(magnitude, phase);
                }




            }
        }

        return input;
    }
}
