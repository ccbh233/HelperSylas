namespace HelperSylas
{
    public static class AnimationAssets
    {
        public const string KingslayerHtml = @"
<!DOCTYPE html>
<html lang='en'>
<head>
    <meta charset='UTF-8'>
    <style>
        body { margin: 0; padding: 0; background-color: #121212; display: flex; justify-content: center; align-items: center; height: 100vh; overflow: hidden; }
        .scene { position: relative; width: 200px; height: 200px; display: flex; justify-content: center; align-items: center; animation: shake 2s infinite; }
        .crown-group { position: absolute; width: 80px; height: 60px; filter: drop-shadow(0 0 5px #FFD700); }
        .shard { fill: url(#goldGradient); stroke: #B8860B; stroke-width: 1; transform-origin: center bottom; }
        .shard-l { animation: shatterL 2s cubic-bezier(0.1, 0.8, 0.2, 1) infinite; }
        .shard-c { animation: shatterC 2s cubic-bezier(0.1, 0.8, 0.2, 1) infinite; }
        .shard-r { animation: shatterR 2s cubic-bezier(0.1, 0.8, 0.2, 1) infinite; }
        .chain-ring { position: absolute; width: 160px; height: 160px; border: 8px dashed #555; border-radius: 50%; box-shadow: 0 0 15px #0AC8B9, inset 0 0 10px #0AC8B9; opacity: 0; animation: smashChain 2s ease-out infinite; }
        .shockwave { position: absolute; width: 10px; height: 10px; border-radius: 50%; background: white; opacity: 0; animation: explode 2s linear infinite; }
        @keyframes smashChain { 0% { transform: scale(1.5) rotate(0deg); opacity: 0; } 40% { transform: scale(1.4) rotate(-20deg); opacity: 0.5; } 50% { transform: scale(0.2) rotate(45deg); opacity: 1; border-color: #fff; } 60% { transform: scale(1.2) rotate(90deg); opacity: 0; } 100% { opacity: 0; } }
        @keyframes shatterL { 0%, 48% { transform: translate(0,0) rotate(0); } 50% { transform: translate(-20px, -10px) rotate(-45deg); } 70% { transform: translate(-25px, -5px) rotate(-50deg); opacity: 0; } 71% { opacity: 0; transform: translate(0,0); } 100% { opacity: 1; } }
        @keyframes shatterR { 0%, 48% { transform: translate(0,0) rotate(0); } 50% { transform: translate(20px, -10px) rotate(45deg); } 70% { transform: translate(25px, -5px) rotate(50deg); opacity: 0; } 71% { opacity: 0; transform: translate(0,0); } 100% { opacity: 1; } }
        @keyframes shatterC { 0%, 48% { transform: translate(0,0); } 50% { transform: translate(0, -30px); } 70% { transform: translate(0, -35px); opacity: 0; } 71% { opacity: 0; transform: translate(0,0); } 100% { opacity: 1; } }
        @keyframes shake { 0%, 49% { transform: translate(0,0); } 50% { transform: translate(-3px, 3px); } 52% { transform: translate(3px, -3px); } 54% { transform: translate(-2px, 2px); } 56% { transform: translate(0,0); } 100% { transform: translate(0,0); } }
        @keyframes explode { 0%, 49% { transform: scale(1); opacity: 0; } 50% { opacity: 1; } 60% { transform: scale(20); opacity: 0; } 100% { opacity: 0; } }
    </style>
</head>
<body>
    <div class='scene'>
        <svg width='0' height='0'><defs><linearGradient id='goldGradient' x1='0%' y1='0%' x2='100%' y2='100%'><stop offset='0%' style='stop-color:#FDB931;stop-opacity:1'/><stop offset='50%' style='stop-color:#AA771C;stop-opacity:1'/><stop offset='100%' style='stop-color:#FDB931;stop-opacity:1'/></linearGradient></defs></svg>
        <div class='chain-ring'></div><div class='shockwave'></div>
        <svg class='crown-group' viewBox='0 0 80 60'>
            <path class='shard shard-l' d='M 25,50 L 25,20 L 10,30 L 5,20 L 10,50 Z' />
            <path class='shard shard-r' d='M 55,50 L 55,20 L 70,30 L 75,20 L 70,50 Z' />
            <path class='shard shard-c' d='M 25,50 L 35,5 L 40,20 L 45,5 L 55,50 Z' />
        </svg>
    </div>
</body>
</html>";
    }
}