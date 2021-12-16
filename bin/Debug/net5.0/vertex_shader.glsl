#version 330

layout(location = 0) in vec3 vPosition;
layout(location = 1) in vec3 vColors;

out vec3 color_in;

uniform mat4 proj; // If you figure it out multiply gl_Position with proj

void main() {
	color_in = vColors;
	gl_Position = vec4(vPosition, 1.0);
}