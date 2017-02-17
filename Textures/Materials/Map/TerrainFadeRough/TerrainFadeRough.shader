Shader "Custom/Terrain hard" {
Properties {
 _MainTex ("Main texture", 2D) = "white" { }
 _Color ("Color", Color) = (1.000000,1.000000,1.000000,1.000000)
}
SubShader { 
 Tags { "RenderType"="Opaque" }
 Pass {
  Tags { "RenderType"="Opaque" }
  ZWrite Off
  GpuProgramID 17546
Program "vp" {
SubProgram "d3d9 " {
GpuProgramIndex 0
}
}
Program "fp" {
SubProgram "d3d9 " {
GpuProgramIndex 1
}
}
 }
}
}