import React from 'react';

const GradientCircle = ({ name, size = 40 }) => {
    // Генерация градиента на основе имени
    const hashCode = (str) => {
        let hash = 0;
        for (let i = 0; i < str.length; i++) {
            hash = str.charCodeAt(i) + ((hash << 5) - hash);
        }
        return hash;
    };

    const intToRGB = (i) => {
        const c = (i & 0x00ffffff).toString(16).toUpperCase();
        return '#' + '00000'.substring(0, 6 - c.length) + c;
    };

    const getGradient = (name) => {
        const hash = hashCode(name);
        const color1 = intToRGB(hash);
        const color2 = intToRGB(hash + 123456); // Добавляем смещение для второго цвета
        return `linear-gradient(135deg, ${color1}, ${color2})`;
    };

    return (
        <div
            className="gradient-circle"
            style={{
                width: `${size}px`,
                height: `${size}px`,
                borderRadius: '50%',
                background: getGradient(name),
                display: 'flex',
                alignItems: 'center',
                justifyContent: 'center',
                color: 'white',
                fontWeight: 'bold',
                fontSize: `${size * 0.4}px`,
            }}
        >
            {name[0].toUpperCase()}
        </div>
    );
};

export default GradientCircle;