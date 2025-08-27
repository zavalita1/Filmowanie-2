import Fireworks from "react-canvas-confetti/dist/presets/fireworks";
import Crossfire from "react-canvas-confetti/dist/presets/crossfire";
import Explosion from "react-canvas-confetti/dist/presets/explosion";
import Photons from "react-canvas-confetti/dist/presets/photons";
import Pride from "react-canvas-confetti/dist/presets/pride";
import Realistic from "react-canvas-confetti/dist/presets/realistic";
import Snow from "react-canvas-confetti/dist/presets/snow";
import Vortex from "react-canvas-confetti/dist/presets/vortex";

export default function Confetti(props: {isEnabled: boolean}) {
    if (!props.isEnabled) {
        return <></>;
    };

    const confettiIndex = Math.floor(Math.random() * confettiMap.length);
    
    return confettiMap[confettiIndex](3);
  }

const confettiMap = [
    (speed: number) => <Fireworks autorun={{speed}} />,
    (speed: number) => <Crossfire autorun={{speed}} />,
    (speed: number) => <Explosion autorun={{speed}} />,
    (speed: number) => <Photons autorun={{speed}} />,
    (speed: number) => <Pride autorun={{speed}} />,
    (speed: number) => <Realistic autorun={{speed}} />,
    (speed: number) => <Snow autorun={{speed}} />,
    (speed: number) => <Vortex autorun={{speed}} />,
];