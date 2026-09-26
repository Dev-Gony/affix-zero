"""Create a self-contained source-sprite review page; never a game/Unity preview.

Only writes Build/Reports/art-preview.html. Source assets remain unchanged.
Requires Pillow through the read-only validator; the HTML needs no server.
"""
from __future__ import annotations

import base64
import json
import sys

sys.dont_write_bytecode = True
from validate_reviewed_art import ROOT, read_record, source_path, validate


HTML = r'''<!doctype html>
<html lang="ko"><meta charset="utf-8"><meta name="viewport" content="width=device-width,initial-scale=1">
<title>AFFIX: ZERO · 원본 애니메이션 검수</title>
<style>
:root{color-scheme:light;font:16px/1.6 system-ui,sans-serif;color:#203b30;background:#edf2e5}*{box-sizing:border-box}body{max-width:1160px;margin:auto;padding:28px}h1{line-height:1.2;font-size:30px;margin:0 0 12px}h2{font-size:20px}.notice{padding:18px 22px;background:#fff3ce;border:1px solid #d5b868;border-radius:16px}.muted{color:#627165;font-size:13px}a{color:#2e7256}button,select{font:inherit;border:1px solid #b5c6b2;border-radius:8px;padding:7px 12px;background:#fff;color:#203b30}.controls{display:flex;gap:12px;flex-wrap:wrap;align-items:center;margin:24px 0;position:sticky;top:0;padding:12px;background:#edf2e5ee;z-index:2}.actors{display:grid;grid-template-columns:1fr 1fr;gap:18px}.actor{background:#fafbf5;padding:20px;border:1px solid #cfddc8;border-radius:18px}.clips{display:grid;grid-template-columns:repeat(2,minmax(0,1fr));gap:12px}.clip{margin:0;background:#dfeacd;border:1px solid #c3d2b7;border-radius:12px;overflow:hidden}.clip canvas{width:100%;aspect-ratio:1;display:block;image-rendering:pixelated}.clip figcaption{padding:8px 12px;background:#f4f7ec}.readout{font-size:12px;color:#516c59}.environment{display:grid;grid-template-columns:repeat(auto-fill,minmax(128px,1fr));gap:12px}.environment figure{margin:0;background:#dfebd0;padding:10px;border-radius:12px;text-align:center}.environment canvas{width:96px;height:96px;image-rendering:pixelated}.environment figcaption{font-size:12px}footer{margin:30px 0;color:#526452}#status{font-weight:600}@media(max-width:740px){body{padding:16px}.actors{grid-template-columns:1fr}.controls{position:static}}
</style>
<header><p class="muted">AFFIX: ZERO / ART INTAKE REVIEW</p><h1>몸과 무기, 실제 프레임 확인</h1>
<div class="notice"><strong>Unity 실행 화면이 아닙니다.</strong> 새 CC0 원본 PNG의 프레임을 재생하는 검사 도구입니다. 게임 전투·피해 판정·Unity import·빌드 검증을 대신하지 않으며 <strong>사용자 시각 승인은 아직 완료되지 않았습니다.</strong></div>
<p>동일한 새 NinjaGreen 캐릭터를 영웅과 적 진영 정찰병으로 사용합니다. 이 페이지는 원본 색을 유지하며, Unity 진영 tint는 보여주지 않습니다. 무기는 공격 클립에서만 표시됩니다.</p></header>
<div class="controls"><label>방향 <select id="direction"><option value="3">오른쪽</option><option value="2">왼쪽</option><option value="0">아래</option><option value="1">위</option></select></label><label>재생 속도 <select id="speed"><option value="1">1×</option><option value="0.5">0.5×</option><option value="0.25">0.25×</option></select></label><button id="pause">일시정지</button><button id="step">한 프레임씩</button><label><input id="anchor" type="checkbox" checked> 발 기준점</label><span id="status">이미지 준비 중</span></div>
<div class="actors" id="actors"></div>
<p class="muted">프레임 번호는 0부터 시작합니다. 공격 impact = 1. 사망은 방향 공용이며 이 검사 도구에서만 반복 재생합니다. 원본에는 4방향이 있지만 첫 Unity 연결은 오른쪽과 좌우 반전을 사용합니다.</p>
<h2>속도와 정렬 검수 기준</h2><p>현재 Unity 연결에 선택한 고정 속도: 영웅 공격 8 fps / 적 공격 10 fps. 양쪽 모두 대기·걷기 8 fps, 피격·사망 10 fps입니다. 이 페이지도 같은 속도로 재생합니다. 이는 프로젝트의 튜닝 값입니다. 제작자의 미리보기 GIF는 IdleWalk 200ms 간격, Attack 200/60/60/100ms 등을 섞는 가변 시간 구조이므로 현재 고정 재생을 공식 원본 타이밍이라고 부르지 않습니다.</p><p class="muted">몸 셀 32px · 무기 셀 64px · PPU 16. 몸 pivot (0.5, 0.25), 무기 pivot (0.5, 0.375)는 같은 발 위치를 가리킵니다. 정지 그림을 움직이는 대신 각 시간 프레임의 실제 몸·무기 픽셀을 선택합니다. ninja-animation.json의 framesPerSecondProposed는 초기 검수 제안값이며 현재 선택 속도와 구별합니다.</p>
<h2>같은 팩의 야외 환경</h2><div class="environment" id="environment"></div>
<footer><p id="validation"></p><a href="https://pixel-boy.itch.io/ninja-adventure-asset-pack">Pixel-Boy / AAA — Ninja Adventure</a> · <a href="https://creativecommons.org/publicdomain/zero/1.0/">CC0 1.0</a><p class="muted">원본 해시·프레임/환경 좌표는 저장소 docs/assets에 기록되어 있습니다. 이 파일에는 원본 PNG가 base64로 포함되어 외부 리소스 요청 없이 재생됩니다.</p></footer>
<script>
const DATA=__DATA__;
const images={};let paused=false, elapsed=0,last=null,ready=false;
const direction=document.querySelector('#direction'),speed=document.querySelector('#speed'),anchor=document.querySelector('#anchor');
const korean={Idle:'대기',Walk:'걷기',Attack:'공격',Hit:'피격',Dead:'사망'};
const views=[];
for(const [name,weapon] of [['영웅 · Katana','Katana'],['근접 정찰병 · Axe','Axe']]){
 const section=document.createElement('section');section.className='actor';section.innerHTML=`<h2>${name}</h2><div class="clips"></div>`;document.querySelector('#actors').append(section);
 for(const clip of DATA.animation.clips){const figure=document.createElement('figure');figure.className='clip';figure.innerHTML=`<canvas width="192" height="192" aria-label="${name} ${korean[clip.name]} 원본 프레임"></canvas><figcaption><strong>${korean[clip.name]}</strong> · ${clip.frames.length} 프레임 <div class="readout"></div></figcaption>`;section.querySelector('.clips').append(figure);views.push({clip,weapon,canvas:figure.querySelector('canvas'),readout:figure.querySelector('.readout'),step:0});}
}
function drawView(view){
 const {canvas,clip,weapon}=view,c=canvas.getContext('2d');c.imageSmoothingEnabled=false;c.clearRect(0,0,192,192);
 const fps=clip.name==='Attack'?(weapon==='Katana'?8:10):(clip.name==='Idle'||clip.name==='Walk'?8:10);
 const frame=paused?view.step:Math.floor(elapsed*fps)%clip.frames.length,col=clip.name==='Dead'?0:Number(direction.value);
 c.fillStyle='#dce9c8';c.fillRect(0,0,192,192);c.fillStyle='#c8d9b2';c.beginPath();c.ellipse(96,122,26,7,0,0,Math.PI*2);c.fill();
 c.drawImage(images[clip.path],col*32,frame*32,32,32,48,48,96,96);
 if(clip.name==='Attack')c.drawImage(images[`Assets/Art/NinjaAdventure/Weapons/${weapon}.png`],col*64,frame*64,64,64,0,0,192,192);
 if(anchor.checked){c.strokeStyle='#45766a';c.lineWidth=1;c.beginPath();c.moveTo(89,120.5);c.lineTo(103,120.5);c.moveTo(96.5,114);c.lineTo(96.5,127);c.stroke();}
 view.readout.textContent=`프레임 ${frame} / ${fps} fps`+(clip.name==='Attack'&&frame===DATA.animation.attackImpactFrameIndex?' · IMPACT':'');
 if(!paused)view.step=frame;
}
function draw(){if(ready)for(const view of views)drawView(view)}
document.querySelector('#pause').onclick=()=>{paused=!paused;document.querySelector('#pause').textContent=paused?'재생':'일시정지';draw()};
document.querySelector('#step').onclick=()=>{paused=true;document.querySelector('#pause').textContent='재생';for(const view of views)view.step=(view.step+1)%view.clip.frames.length;draw()};
direction.onchange=draw;anchor.onchange=draw;
function tick(now){if(last!==null&&!paused)elapsed+=Math.min((now-last)/1000,.1)*Number(speed.value);last=now;draw();requestAnimationFrame(tick)}
Promise.all(Object.entries(DATA.images).map(([path,src])=>new Promise((resolve,reject)=>{const image=new Image();image.onload=()=>{images[path]=image;resolve()};image.onerror=()=>reject(new Error(path));image.src=src}))).then(()=>{
 for(const sprite of DATA.environment.sprites){const figure=document.createElement('figure');figure.innerHTML=`<canvas width="96" height="96" aria-label="${sprite.name}"></canvas><figcaption>${sprite.name}<br>${sprite.width} × ${sprite.height}px</figcaption>`;document.querySelector('#environment').append(figure);const c=figure.querySelector('canvas').getContext('2d');c.imageSmoothingEnabled=false;const scale=Math.min(3,80/sprite.width,80/sprite.height),w=sprite.width*scale,h=sprite.height*scale;c.drawImage(images[sprite.path],sprite.x,sprite.y,sprite.width,sprite.height,(96-w)/2,(96-h)/2,w,h);}
 ready=true;document.querySelector('#status').textContent='원본 준비 완료';document.querySelector('#validation').textContent=`원본 검증 PASS · ${DATA.validation.sourceFiles}개 파일 SHA-256 · ${DATA.validation.auditedRightFacingFrames}개 프레임 픽셀 · 환경 ${DATA.validation.environmentSprites}개 영역`;requestAnimationFrame(tick);
}).catch(error=>{document.querySelector('#status').textContent='이미지 로드 실패: '+error.message;console.error(error)});
</script></html>'''


def main() -> None:
    validation = validate()
    animation = read_record("ninja-animation.json")
    environment = read_record("ninja-environment.json")
    paths = {clip["path"] for clip in animation["clips"]}
    paths.update(sprite["path"] for sprite in environment["sprites"])
    paths.update(f"Assets/Art/NinjaAdventure/Weapons/{name}.png" for name in ("Katana", "Axe"))
    images = {path: "data:image/png;base64," + base64.b64encode(source_path(path).read_bytes()).decode("ascii") for path in sorted(paths)}
    data = json.dumps(dict(validation=validation, animation=animation, environment=environment, images=images), ensure_ascii=False).replace("<", "\\u003c")
    output = ROOT / "Build/Reports/art-preview.html"
    output.parent.mkdir(parents=True, exist_ok=True)
    output.write_text(HTML.replace("__DATA__", data), encoding="utf-8")
    print(json.dumps({"status": "PASS", "output": str(output), "bytes": output.stat().st_size,
                      "unityExecuted": False, "visualApproval": "NOT_RUN"}, ensure_ascii=False, indent=2))


if __name__ == "__main__":
    main()
