interface PlaceholderProps {
  title: string;
}

export const Placeholder = ({ title }: PlaceholderProps) => {
  return (
    <div className="flex flex-col items-center justify-center h-full min-h-[50vh] text-center">
      <h1 className="text-3xl font-bold text-gray-900 mb-4">{title}</h1>
      <p className="text-gray-500 max-w-md">
        Bu sayfa henüz yapım aşamasındadır. Yakında burada harika özellikler olacak!
      </p>
    </div>
  );
};
