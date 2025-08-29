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
  dialogCancelText: string;
  dialogActionText?: string;
  className?: string;
  isLarge?: boolean;
};

export const ConfirmationDialog: React.FC<DialogProps> = props => {
  const className = clsx([
    "w-full",
    "p-6",
    "flex",
    "justify-center",
    props.className ?? ""
  ])

  return (<div className={className}>
      <AlertDialog open={props.isOpen}>
        <AlertDialogContent className={props.isLarge ? "large-dialog": ""}>
          <AlertDialogHeader>
            <AlertDialogTitle>{props.dialogTitle}</AlertDialogTitle>
            <AlertDialogDescription>
              {props.dialogContent}
            </AlertDialogDescription>
          </AlertDialogHeader>
          <AlertDialogFooter>
            <AlertDialogCancel className="cursor-pointer max-w-70 text-balance md:max-w-100 bg-emerald-50" onClick={props.onClose}>
                { portionButtonText(props.dialogCancelText) }
              </AlertDialogCancel>
            { props.dialogActionText 
              ? <AlertDialogAction className="cursor-pointer bg-emerald-700" onClick={props.onAction}> {props.dialogActionText} </AlertDialogAction>
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