import * as React from 'react';
import { styled } from '@mui/material/styles';
import Rating, { IconContainerProps } from '@mui/material/Rating';
import SentimentSatisfiedIcon from '@mui/icons-material/SentimentSatisfied';
import SentimentSatisfiedAltIcon from '@mui/icons-material/SentimentSatisfiedAltOutlined';
import SentimentVerySatisfiedIcon from '@mui/icons-material/SentimentVerySatisfied';
import DeleteOutlineOutlinedIcon from '@mui/icons-material/DeleteOutlineOutlined';

const StyledRating = styled(Rating)(({ theme }) => ({
  '& .MuiRating-iconEmpty .MuiSvgIcon-root': {
    color: theme.palette.action.disabled,
  },
}));

const customIcons: IIconDictionary = {
  1: {
    icon: <DeleteOutlineOutlinedIcon fontSize="large" className='trash-icon'/>,
    label: 'Very Dissatisfied',
    originalKey: 1
  },
  2: {
    icon: <SentimentSatisfiedIcon className='happy-icon' fontSize="large"/>,
    label: 'Neutral',
    originalKey: 2
  },
  3: {
    icon: <SentimentSatisfiedAltIcon className='very-happy-icon' fontSize="large"/>,
    label: 'Satisfied',
    originalKey: 3
  },
  4: {
    icon: <SentimentVerySatisfiedIcon className='ecstatic-icon' fontSize="large"/>,
    label: 'Very Satisfied',
    originalKey: 4
  },
};

function IconContainer(props: IconContainerProps & IIconsAvailability) {
  const { value, ...other } = props;
  const icons = getAvailableIcons(props);

  return <span {...other}>{icons[value].icon}</span>;
}

function getIconContainer(iconsAvailability: IIconsAvailability) {
  return ((props: IconContainerProps) => {
    const containerProps = {... props, ...iconsAvailability};
    return <IconContainer {...containerProps} />
});
}

export default function RadioGroupRating(props: { onChange: (iconChange: number) => void, iconsAvailability: IIconsAvailability}) {
  const max = props.iconsAvailability.availableIconsIndices.length;
  const defaultValue = max === 1 && props.iconsAvailability.chosenIndex !== undefined ? 1 : null;

  return (
    <StyledRating
      name="highlight-selected-only"
      IconContainerComponent={getIconContainer(props.iconsAvailability)}
      getLabelText={(value: number) => customIcons[value].label}
      highlightSelectedOnly
      onChange={onChange}
      max={max}
      value={defaultValue}
    />
  );

  function onChange(_: React.SyntheticEvent, value: number | null) {
    if (value != null) {
      const icons = getAvailableIcons(props.iconsAvailability);
      props.onChange(icons[value].originalKey);
    }
    else {
      props.onChange(-1);
    }
  }
}

export interface IIconsAvailability {
  chosenIndex?: number
  availableIconsIndices: number[]
}

interface IIconDictionary {
    [index: string]: {
      icon: React.ReactElement;
      label: string;
      originalKey: number;
    };
}

function getAvailableIcons(iconsAvailability: IIconsAvailability) {
  const icons: IIconDictionary = {};
  let counter = 1;
  let availableIconsCounter = 1;
  for (const icon in customIcons) {
    if (iconsAvailability.availableIconsIndices.includes(counter)) {
      icons[availableIconsCounter++] = customIcons[icon];
    }
    counter++;
  }

  return icons;
}
