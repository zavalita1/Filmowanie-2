import clsx from "clsx";
import { AlertDialog, AlertDialogCancel, AlertDialogAction, AlertDialogContent, AlertDialogDescription, AlertDialogFooter, AlertDialogHeader, AlertDialogTitle,
} from "./ui/alert-dialog"
import { ReactNode } from "react";

type DialogProps = {
  onClose: () => void;
  onAction?: () => void;
  isOpen: boolean;
  dialogContent: string | ReactNode;
  dialogTitle: string;
  dialogSubtitle?: string;
  dialogCancelText: string;
  dialogActionText?: string;
  className?: string;
  isLarge?: boolean;
  isMobile: boolean;
};

export const ConfirmationDialog: React.FC<DialogProps> = props => {
  const className = clsx([
    "w-full",
    props.isMobile ? "p-1" : "p-6",
    "flex",
    "text-justify",
    "justify-center",
    "text-amber-200",
    props.className ?? ""
  ]);

  const alertDialogActionClassname = clsx([
    "cursor-pointer",
    "bg-emerald-700",
    "dark:bg-pink-900",
    props.isMobile ? "self-center w-4/5" : ""
  ]);

  const alertCancelDialogClassname = clsx([
    "cursor-pointer",
    "max-w-4/5",
    "text-balance",
    "md:max-w-100",
    "bg-emerald-50",
    "dark:bg-pink-800",
    props.isMobile ? "self-center w-4/5" : ""
  ]);

  return (<div className={className}>
      <AlertDialog open={props.isOpen}>
        <AlertDialogContent className={props.isLarge ? "large-dialog": "p-3"}>
          <AlertDialogHeader className="">
            <AlertDialogTitle>{props.dialogTitle}</AlertDialogTitle>
            { props.dialogSubtitle === undefined ? <></> : <div>{props.dialogSubtitle}</div> }
            <AlertDialogDescription className="select-none">
              {props.dialogContent}
            </AlertDialogDescription>
          </AlertDialogHeader>
          <AlertDialogFooter className="justify-center">
            <AlertDialogCancel className={alertCancelDialogClassname} onClick={props.onClose}>
                { portionButtonText(props.dialogCancelText) }
              </AlertDialogCancel>
            { props.dialogActionText 
              ? <AlertDialogAction className={alertDialogActionClassname} onClick={props.onAction}> {props.dialogActionText} </AlertDialogAction>
              : <></>
            }
          </AlertDialogFooter>
        </AlertDialogContent>
      </AlertDialog>
    </div>);
}

function portionButtonText(text: string) {
  if (text.length < 10) {
    return <> {text}</>
  }
  else {
    const words = text.split(' ');
    const midPoint = (words.length - (words.length % 2)) / 2;
    const line1 = words.slice(0, midPoint).join(" ");
    const line2 = words.slice(midPoint).join(" ");
    return <>{line1} <br/> {line2}</>
  }
}