import { Movie, PlaceholderMovie, ConcreteMovie } from "../../models/Movie";
import { AppComponentProps } from "../../pages/Layout";
import { Card, CardHeader, CardDescription, CardTitle, CardContent, CardFooter } from "./card";
import { Drawer, DrawerClose, DrawerContent, DrawerDescription, DrawerFooter, DrawerHeader, DrawerTitle, DrawerTrigger } from "./drawer";

export type MovieCardProps = AppComponentProps & {
  movie: Movie;
}

export const MovieCard: React.FC<MovieCardProps> = props => {
  const placeholderMovie = props.movie as PlaceholderMovie;
  if (!!placeholderMovie?.decade) {
    let message = `${placeholderMovie.title} ${placeholderMovie.decade}`;
    message = message.slice(0, -1) + 'X';
    return (
      <Card className="w-3xs m-2 mr-10 justify-center bg-gray-100">
        <CardHeader>
          <CardDescription className="text-2xl text-neutral-950 text-center"><b>{placeholderMovie.title}</b></CardDescription>
        </CardHeader>
      </Card>
    );
  }
  
  const concreteMovie = props.movie as ConcreteMovie;

  return (
    <Drawer>
    <DrawerTrigger asChild={true}>
      <Card className="w-3xs m-2 mr-10  hover:bg-blue-200 hover:cursor-pointer">
        <CardHeader>
          <CardTitle><b className="text-2xl">{concreteMovie.movieName}</b></CardTitle>
          <CardDescription>{concreteMovie.genres.join(", ")}</CardDescription>
        </CardHeader>
        <CardContent>
          <img className="justify-self-center" src={concreteMovie.posterUrl}></img>
        </CardContent>
        {/* <CardFooter>
          <p>Card Footer</p>
        </CardFooter> */}
      </Card>
    </DrawerTrigger>
    <DrawerContent>
      <div className="mx-auto w-full max-w-5/6 min-h-96">
         <DrawerHeader>
            <DrawerTitle>
              <p className="text-5xl mb-10"><b>{concreteMovie.movieName}</b></p>
              </DrawerTitle>
            <DrawerDescription className="text-xl text-justify">
              <div className="flex mb-10">
                <img className="h-fit mr-10" src="https://fwcdn.pl/fpo/00/33/120033/7606010_1.8.webp"></img>
                <div className="block">
                  <p className="mb-10">{concreteMovie.description}</p>
                  <div>
                    <p><b>Metraż:</b> <i>{concreteMovie.duration}</i></p>
                    <p><b>Gatunek:</b> {concreteMovie.genres.join(", ")}</p>
                    <p><b>Reżyseria:</b> {concreteMovie.directors.join(", ")}</p>
                    <p><b>Scenariusz:</b> {concreteMovie.writers.join(", ")}</p>
                    <p><b>Występują:</b> {concreteMovie.actors.join(", ")}</p>
                    <p className="mt-4 text-center text-blue-700"><a href={concreteMovie.filmwebUrl} target="_blank">Link do filmweba.</a></p>
                  </div>
                </div>
              </div>
            </DrawerDescription>
          </DrawerHeader>
      </div>
    </DrawerContent>
    </Drawer>
  );
}