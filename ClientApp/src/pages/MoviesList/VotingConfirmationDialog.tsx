import {
  AlertDialog,
  AlertDialogAction,
  AlertDialogCancel,
  AlertDialogContent,
  AlertDialogDescription,
  AlertDialogFooter,
  AlertDialogHeader,
  AlertDialogTitle,
} from "../../components/ui/alert-dialog"

type VotingConfirmationDialogProps = {
  onClose: () => void;
  isOpen: boolean;
  dialogText: string;
  dialogTitle: string;
  dialogConfirmationText: string;
};

export const VotingConfirmationDialog: React.FC<VotingConfirmationDialogProps> = props => {
  return (<div className="w-full p-6 flex justify-center">
      <AlertDialog open={props.isOpen}>
        <AlertDialogContent>
          <AlertDialogHeader>
            <AlertDialogTitle>{props.dialogTitle}</AlertDialogTitle>
            <AlertDialogDescription>
              {props.dialogText}
            </AlertDialogDescription>
          </AlertDialogHeader>
          <AlertDialogFooter>
            <AlertDialogCancel onClick={() => props.onClose()}>{props.dialogConfirmationText}</AlertDialogCancel>
          </AlertDialogFooter>
        </AlertDialogContent>
      </AlertDialog>
    </div>);
}