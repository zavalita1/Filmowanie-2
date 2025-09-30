import { useState } from "react";
import { Checkbox } from "../components/ui";
import { AppComponentProps, Layout } from "./Layout";
import { userPreferencesSlice } from "../store/slices/userPreferencesSlice";
import { useAppSelector, useAppDispatch } from "../hooks/redux";
import { BasicLogin } from '../components/BasicLogin';
import { Accordion, AccordionContent, AccordionItem, AccordionTrigger } from "../components/ui/accordion";
import { LoginWithBasicAuthOutgoingDTO } from '../store/apis/1-User/types';
import { Separator } from '../components/ui';
import { useSignUpMutation } from "../store/apis/1-User/userApi";
import { useNavigate } from 'react-router';


const Preferences: React.FC<AppComponentProps> = props => {
    const userPreferences = useAppSelector(x => x.userPreferences);
    const dispatch = useAppDispatch();
    const [preferSimplifiedCardView, setPreferSimplifiedCardView] = useState(userPreferences.preferSimplifiedCardView);
    const [preferAltMovieDescriptions, setPreferAltMovieDescriptions] = useState(userPreferences.preferAltMovieDescriptions);
    const [useSignUp, result] = useSignUpMutation();
    const navigate = useNavigate();
    
    return (
        <div className="self-baseline flex flex-wrap mt-10 md:mt-50">
            <Checkbox
                    id="toggle-2"
                    defaultChecked={false}
                    checked={preferSimplifiedCardView}
                    onCheckedChange={onSetPreferSimplifiedView}
                    className="mr-2 data-[state=checked]:border-emerald-400 data-[state=checked]:bg-emerald-400 data-[state=checked]:text-white dark:data-[state=checked]:border-pink-900 dark:data-[state=checked]:bg-pink-900"
                />
                 <div className="grid gap-1.5 font-normal mb-5 max-w-xs md:max-w-3xl">
                    <p className="text-sm leading-none font-medium">
                        Używaj uproszczonego widoku kart.
                    </p>
                    <p className="text-muted-foreground text-sm text-wrap max-w-xs md:max-w-3xl">
                        Zaznacz, jeżeli nie lubisz dużo klikać i 
                        chcesz widzieć wszystko od razu na ekranie głosowania.
                    </p>
                </div>
                <div className="h-0 basis-full"></div>
                <Checkbox
                    id="toggle-2"
                    defaultChecked={false}
                    checked={preferAltMovieDescriptions}
                    onCheckedChange={onSetPreferAltDescription}
                    className="mr-2 data-[state=checked]:border-emerald-400 data-[state=checked]:bg-emerald-400 data-[state=checked]:text-white dark:data-[state=checked]:border-pink-900 dark:data-[state=checked]:bg-pink-900"
                />
                 <div className="grid gap-1.5 font-normal">
                    <p className="text-sm leading-none font-medium">
                        Używaj alternatywnych opisów filmów.
                    </p>
                    <p className="text-muted-foreground text-sm">
                        Istnieje ryzyko drobnych spoilerów.
                    </p>
                </div>
                {props.userData!.hasRegisteredBasicAuth === true ? <></> :
                <>
                    <Separator className='mt-10 mb-5' />
                    <Accordion
                        type="single"
                        collapsible
                        className="w-full md:w-4/5 flex"
                        defaultValue="item-2"
                    >
                        <AccordionItem className='h-[200px] md:w-3/5' value="item-1">
                            <AccordionTrigger>Opcjonalne ustawiania swojego hasła - możesz, nie musisz.</AccordionTrigger>
                            <AccordionContent className="text-balance">
                                <p className="mb-2">
                                    Strudzeni szukaniem kodu za każdym razem? Możecie wstukać mail + hasło, co by łatwiej pamiętać.
                                </p>
                                <BasicLogin submitText='Wstukaj' onSubmit={signUp} pwdPlaceholderText="Twoje hasło"></BasicLogin>
                            </AccordionContent>
                        </AccordionItem>
                    </Accordion>
                </>
            }
        </div>
    );


    async function signUp(email: string, password: string) {
        const dto: LoginWithBasicAuthOutgoingDTO = { email, password };
        await useSignUp(dto).unwrap();
        navigate('/login');
      }

    function onSetPreferSimplifiedView() {
        setPreferSimplifiedCardView(!preferSimplifiedCardView);
        const action = userPreferencesSlice.actions.setPreferSimplifiedCardView(!preferSimplifiedCardView);
        dispatch(action);
    }

    function onSetPreferAltDescription() {
        setPreferAltMovieDescriptions(!preferAltMovieDescriptions);
        const action = userPreferencesSlice.actions.setPreferAltMovieDescriptions(!preferAltMovieDescriptions);
        dispatch(action);
    }
}

const wrappedHome: React.FC<AppComponentProps> = (props) => { return <Layout><Preferences {...props}/></Layout>}

export default wrappedHome;