import {
  atsysApi,
  CreateLectureCommand,
  LectureDto,
  UpdateLectureCommand,
  usePostApiVbyVersionLectureCreateMutation,
  usePutApiVbyVersionLectureUpdateMutation,
} from "@/redux/generatedTypes";
import {
  Dialog,
  DialogBackdrop,
  DialogPanel,
  DialogTitle,
} from "@headlessui/react";
import React, { useEffect, useState } from "react";
import { toast, Toaster } from "react-hot-toast";

const AddOrUpdateLectureModal = ({
  id,
  onClose,
  isOpen,
  refetch,
}: {
  id: number | null;
  onClose: () => void;
  isOpen: boolean;
  refetch: () => void;
}) => {
  const [trigger, { isLoading }] =
    atsysApi.endpoints.getApiVbyVersionLectureGetByIdAndId.useLazyQuery();
  const [updateLectureAsync] = usePutApiVbyVersionLectureUpdateMutation();
  const [addLectureAsync] = usePostApiVbyVersionLectureCreateMutation();

  const [lecture, setLecture] = useState<LectureDto | null>({
    id: undefined,
    code: "",
    name: "",
    description: "",
  });

  useEffect(() => {
    if (isOpen && id) {
      trigger({
        id: id,
        version: process.env.NEXT_PUBLIC_API_VERSION as string,
      })
        .unwrap()
        .then((res) => {
          if (res.success) {
            setLecture(res?.data ?? null);
          } else {
            toast.error(
              `Failed to fetch lecture details: ${res?.message} \n ${res?.errors?.join(", ")}`
            );
            setLecture(null);
          }
        })
        .catch((error) => {
          toast.error(`Error when fetching lecture details: ${error.message}`);
        });
    } else {
      setLecture({
        id: undefined,
        code: "",
        name: "",
        description: "",
      });
    }
  }, [isOpen, id, trigger]);

  const handleAddOrUpdateLecture = async () => {
    if (id) {
      if (!lecture) return;
      const updatedLecture: UpdateLectureCommand = {
        id: lecture.id,
        code: lecture.code,
        name: lecture.name,
        description: lecture.description,
      };

      const promise = updateLectureAsync({
        updateLectureCommand: updatedLecture,
        version: process.env.NEXT_PUBLIC_API_VERSION as string,
      }).unwrap();

      toast.promise(
        async () => {
          const res = await promise;
          return res;
        },
        {
          loading: "Updating lecture...",
          success: (res) => {
            if (res?.success) {
              refetch();
              onClose();
              return res?.message ?? "Lecture updated successfully";
            } else {
              throw new Error(
                `${res?.message ?? ""} \n ${res.errors?.join(", ")}`
              );
            }
          },
          error: (error) => {
            return `Failed to update lecture: ${error.message}`;
          },
        }
      );
    } else {
      const newLecture: CreateLectureCommand = {
        code: lecture?.code || "",
        name: lecture?.name || "",
        description: lecture?.description || "",
      };

      const promise = addLectureAsync({
        createLectureCommand: newLecture,
        version: process.env.NEXT_PUBLIC_API_VERSION as string,
      }).unwrap();

      toast.promise(
        async () => {
          const res = await promise;
          return res;
        },
        {
          loading: "Adding lecture...",
          success: (res) => {
            if (res?.success) {
              refetch();
              onClose();
              return res?.message ?? "Lecture added successfully";
            } else {
              throw new Error(
                `${res?.message ?? ""} \n ${res.errors?.join(", ")}`
              );
            }
          },
          error: (error) => {
            return `Failed to add lecture: ${error.message}`;
          },
        }
      );
    }
  };

  return (
    <>
      <Toaster
        position="top-right"
        toastOptions={{
          className: "toast",
          duration: 3000,
        }}
      />
      <Dialog open={isOpen} onClose={onClose} className="relative z-10">
        {isLoading ? (
          <svg
            className={`animate-spin w-8 h-8 text-blue-500`}
            xmlns="http://www.w3.org/2000/svg"
            fill="none"
            viewBox="0 0 24 24"
            role="status"
            aria-label="Loading"
          >
            <circle
              className="opacity-25"
              cx="12"
              cy="12"
              r="10"
              stroke="currentColor"
              strokeWidth="4"
            />
            <path
              className="opacity-75"
              fill="currentColor"
              d="M4 12a8 8 0 018-8V0C5.373 0 0 5.373 0 12h4z"
            />
          </svg>
        ) : (
          <>
            <DialogBackdrop
              transition
              className="fixed inset-0 bg-gray-500/75 transition-opacity data-closed:opacity-0 data-enter:duration-300 data-enter:ease-out data-leave:duration-200 data-leave:ease-in"
            />

            <div className="fixed inset-0 z-10 w-screen overflow-y-auto">
              <div className="flex min-h-full items-end justify-center p-4 text-center sm:items-center sm:p-0">
                <DialogPanel
                  transition
                  className="relative transform overflow-hidden rounded-lg bg-white text-left shadow-xl transition-all data-closed:translate-y-4 data-closed:opacity-0 data-enter:duration-300 data-enter:ease-out data-leave:duration-200 data-leave:ease-in sm:my-8 sm:w-full sm:max-w-lg data-closed:sm:translate-y-0 data-closed:sm:scale-95"
                >
                  <form
                    className="w-full max-w-lg px-4 pt-5 pb-4 sm:p-6 sm:pb-4"
                    onSubmit={(e) => {
                      e.preventDefault();
                      handleAddOrUpdateLecture();
                    }}
                  >
                    <DialogTitle
                      as="h3"
                      className="text-base font-semibold text-gray-900 mb-4"
                    >
                      {id ? "Update Lecture" : "Add Lecture"}
                    </DialogTitle>

                    {/* Code & Name row */}
                    <div className="flex flex-wrap -mx-3 mb-6">
                      {/* Code */}
                      <div className="w-full md:w-1/2 px-3 mb-6 md:mb-0">
                        <label
                          htmlFor="code"
                          className="block uppercase tracking-wide text-gray-700 text-xs font-bold mb-2"
                        >
                          Code
                        </label>
                        <input
                          id="code"
                          type="text"
                          className="appearance-none block w-full bg-gray-50 border border-gray-200 rounded py-3 px-4 leading-tight focus:outline-none focus:bg-white focus:border-gray-500"
                          value={lecture?.code || ""}
                          onChange={(e) =>
                            setLecture((prev) => ({
                              ...prev!,
                              code: e.target.value,
                            }))
                          }
                          required
                        />
                      </div>

                      {/* Name */}
                      <div className="w-full md:w-1/2 px-3">
                        <label
                          htmlFor="name"
                          className="block uppercase tracking-wide text-gray-700 text-xs font-bold mb-2"
                        >
                          Name
                        </label>
                        <input
                          id="name"
                          type="text"
                          className="appearance-none block w-full bg-gray-50 border border-gray-200 rounded py-3 px-4 leading-tight focus:outline-none focus:bg-white focus:border-gray-500"
                          value={lecture?.name || ""}
                          onChange={(e) =>
                            setLecture((prev) => ({
                              ...prev!,
                              name: e.target.value,
                            }))
                          }
                          required
                        />
                      </div>
                    </div>

                    {/* Description row */}
                    <div className="flex flex-wrap -mx-3 mb-6">
                      <div className="w-full px-3">
                        <label
                          htmlFor="description"
                          className="block uppercase tracking-wide text-gray-700 text-xs font-bold mb-2"
                        >
                          Description
                        </label>
                        <textarea
                          id="description"
                          className="appearance-none block w-full bg-gray-50 border border-gray-200 rounded py-3 px-4 leading-tight focus:outline-none focus:bg-white focus:border-gray-500"
                          rows={4}
                          value={lecture?.description || ""}
                          onChange={(e) =>
                            setLecture((prev) => ({
                              ...prev!,
                              description: e.target.value,
                            }))
                          }
                        />
                      </div>
                    </div>

                    {/* action buttons */}
                    <div className="px-4 py-3 sm:flex sm:flex-row-reverse sm:px-6">
                      <button
                        type="submit"
                        className="inline-flex w-full justify-center rounded-md bg-blue-600 px-4 py-2 text-white hover:bg-blue-700 sm:ml-3 sm:w-auto sm:text-sm"
                      >
                        {id ? "Update Lecture" : "Add Lecture"}
                      </button>
                      <button
                        type="button"
                        className="mt-3 inline-flex w-full justify-center rounded-md bg-white px-4 py-2 text-gray-700 hover:bg-gray-50 sm:mt-0 sm:ml-3 sm:w-auto sm:text-sm"
                        onClick={onClose}
                      >
                        Cancel
                      </button>
                    </div>
                  </form>
                </DialogPanel>
              </div>
            </div>
          </>
        )}
      </Dialog>
    </>
  );
};

export default AddOrUpdateLectureModal;
