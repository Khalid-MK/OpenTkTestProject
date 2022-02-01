#version 440 core

// In
in vec4 frag_color;

// Uniform

//Out
out vec4 color;

void main(void)
{
	color = frag_color;
}