using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace UltraShaderCore
{
    // Only listed for windows
    public enum UnityPlatforms
    {
        Direct3D9 = 1,
        Direct3D11 = 2,
        Direct3D12 = 18,
        OpenGLCore = 17,
        OpenGLES2 = 8,
        OpenGLES3 = 11,
        Vulkan = 21
    }

    public enum ShaderPlatform
    {
        OpenGL,
        Direct3D9,
        Xbox360,
        PS3,
        Direct3D11,
        OpenGLES,
        OpenGLESPC,
        Flash,
        Direct3D11_9x,
        OpenGLES3,
        PSV,
        PS4,
        XBox1,
        PSM,
        Metal,
        OpenGLCore,
        ThreeDS,
        WiiU,
        Vulkan,
        Switch
    }

    public enum ShaderType
    {
        Unknown,
        OpenGL,
        OpenGLES31AEP,
        OpenGLES31,
        OpenGLES3,
        OpenGLES,

        OpenGLCore32,
        OpenGLCore41,
        OpenGLCore43,

        Direct3D9_Vertex_20,
        Direct3D9_Vertex_30,
        Direct3D9_Pixel_20,
        Direct3D9_Pixel_30,

        Direct3D11_9x_Vertex,
        Direct3D11_9x_Pixel,

        Direct3D11_Vertex_40,
        Direct3D11_Vertex_50,
        Direct3D11_Pixel_40,
        Direct3D11_Pixel_50,
        Direct3D11_Geometry_40,
        Direct3D11_Geometry_50,
        Direct3D11_Hull_50,
        Direct3D11_Domain_50,
        Metal_Vertex,
        Metal_Pixel
    }

    //from AS
    public enum SerializedPropertyType
    {
        Color = 0,
        Vector = 1,
        Float = 2,
        Range = 3,
        Texture = 4
    }

    public enum TextureDimension
    {
        TexDimUnknown = -1,
        TexDimNone = 0,
        TexDimAny = 1,
        TexDim2D = 2,
        TexDim3D = 3,
        TexDimCUBE = 4,
        TexDim2DArray = 5,
        TexDimCubeArray = 6,
        TexDimForce32Bit = 0x7fffffff
    }
    /////////////
}
