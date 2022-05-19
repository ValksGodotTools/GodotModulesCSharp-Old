shader_type canvas_item;
uniform vec2 dir = vec2(0,0);
uniform int quality = 4;

void vertex(){
	vec2 blurSize = abs(dir) * 2.0;
	VERTEX *= blurSize + 1.0;
	UV = (UV - 0.5) * (blurSize + 1.0) + 0.5;
}

float insideUnitSquare(vec2 v) {
    vec2 s = step(vec2(0.0), v) - step(vec2(1.0), v);
    return s.x * s.y;   
}

void fragment(){
	float inSquare = insideUnitSquare(UV);
	float numSamples = inSquare;
	COLOR = texture(TEXTURE, UV) * inSquare;
	vec2 stepSize = dir/(float(quality));
	vec2 uv;
	for(int i = 1; i <= quality; i++){
		uv = UV + stepSize * float(i);
		inSquare = insideUnitSquare(uv);
		numSamples += inSquare;
		COLOR += texture(TEXTURE, uv) * inSquare;
		
		uv = UV - stepSize * float(i);
		inSquare = insideUnitSquare(uv);
		numSamples += inSquare;
		COLOR += texture(TEXTURE, uv) * inSquare;
	}
	COLOR.rgb /= numSamples;
	COLOR.a /= float(quality)*2.0 + 1.0;
}
