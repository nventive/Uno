﻿namespace Uno.UI {

	export class WindowManager {

		public static current: WindowManager;

		private static readonly unoRootClassName = "uno-root-element";
		private static readonly unoUnarrangedClassName = "uno-unarranged";

		/**
			* Initialize the WindowManager
			* @param containerElementId The ID of the container element for the Xaml UI
			* @param loadingElementId The ID of the loading element to remove once ready
			*/
		public static init(containerElementId: string = "uno-body", loadingElementId: string = "uno-loading"): string {

			if (WindowManager.assembly) {
				throw "Already initialized";
			}
			WindowManager.initMethods();

			this.current = new WindowManager(containerElementId, loadingElementId);
			this.current.init();
			return "ok";
		}

		private containerElement: HTMLDivElement;
		private rootContent: HTMLElement;

		private allActiveElementsById: { [name: string]: HTMLElement | SVGElement } = {};

		static assembly: UI.Interop.IMonoAssemblyHandle;
		private static resizeMethod: UI.Interop.IMonoMethodHandle;
		private static dispatchEventMethod: UI.Interop.IMonoMethodHandle;

		private constructor(private containerElementId: string, private loadingElementId: string) {
			this.initDom();
		}

 		/**
			* Creates the UWP-compatible splash screen
            * 
			*/
		static setupSplashScreen(): void {

			if (UnoAppManifest && UnoAppManifest.splashScreenImage) {

				const loading = document.getElementById("loading");

				if (loading) {
					loading.remove();
				}

				const unoBody = document.getElementById("uno-body");

				if (unoBody) {
					const unoLoading = document.createElement("div");
					unoLoading.id = "uno-loading";

					if (UnoAppManifest.splashScreenColor) {
						const body = document.getElementsByTagName("body")[0];
						body.style.backgroundColor = UnoAppManifest.splashScreenColor;
					}

					const unoLoadingSplash = document.createElement("div");
					unoLoadingSplash.id = "uno-loading-splash";
					unoLoadingSplash.classList.add("uno-splash");
					unoLoadingSplash.style.backgroundImage = `url('${UnoAppManifest.splashScreenImage}')`;

					unoLoading.appendChild(unoLoadingSplash);

					unoBody.appendChild(unoLoading);
				}
			}
        }

 		/**
			* Reads the window's search parameters
            * 
			*/
        static findLaunchArguments(): string {
            if (typeof URLSearchParams === "function") {
                return new URLSearchParams(window.location.search).toString();
            }
            else {
                let queryIndex = document.location.search.indexOf('?');

                if (queryIndex != -1) {
                    return document.location.search.substring(queryIndex + 1);
                }

                return "";
            }
        }

		/**
			* Create a html DOM element representing a Xaml element.
			*
			* You need to call addView to connect it to the DOM.
			*/
		public createContent(contentDefinition: IContentDefinition): string {
			// Create the HTML element
			const element =
				contentDefinition.isSvg
					? document.createElementNS("http://www.w3.org/2000/svg", contentDefinition.tagName)
					: document.createElement(contentDefinition.tagName);
			element.id = contentDefinition.id;
			element.setAttribute("XamlType", contentDefinition.type);
			element.setAttribute("XamlHandle", `${contentDefinition.handle}`);
			if (contentDefinition.isFrameworkElement) {
				element.classList.add(WindowManager.unoUnarrangedClassName);
			}
			if (element.hasOwnProperty("tabindex")) {
				(element as any)["tabindex"] = contentDefinition.isFocusable ? 0 : -1;
			} else {
				element.setAttribute("tabindex", contentDefinition.isFocusable ? '0' : '-1');
			}

			if (contentDefinition) {
				for (const className of contentDefinition.classes) {
					element.classList.add(`uno-${className}`);
				}
			}

			// Add the html element to list of elements
			this.allActiveElementsById[contentDefinition.id] = element;

			return "ok";
		}

		/**
			* Set a name for an element.
			*
			* This is mostly for diagnostic purposes.
			*/
		public setName(elementId: string, name: string): string {
			const htmlElement: HTMLElement | SVGElement = this.allActiveElementsById[elementId];
			if (!htmlElement) {
				throw `Element id ${elementId} not found.`;
			}

			htmlElement.setAttribute("XamlName", name);

			return "ok";
		}

		/**
			* Set an attribute for an element.
			*/
		public setAttribute(elementId: string, attributes: { [name: string]: string }): string {
			const htmlElement: HTMLElement | SVGElement = this.allActiveElementsById[elementId];
			if (!htmlElement) {
				throw `Element id ${elementId} not found.`;
			}


			for (const name in attributes) {
				if (attributes.hasOwnProperty(name)) {
					htmlElement.setAttribute(name, attributes[name]);
				}
			}

			return "ok";
		}

		/**
			* Get an attribute for an element.
			*/
		public getAttribute(elementId: string, name: string): any {
			const htmlElement: HTMLElement | SVGElement = this.allActiveElementsById[elementId];
			if (!htmlElement) {
				throw `Element id ${elementId} not found.`;
			}

			return htmlElement.getAttribute(name);
		}

		/**
			* Set a property for an element.
			*/
		public setProperty(elementId: string, properties: { [name: string]: string }): string {
			const htmlElement: HTMLElement | SVGElement = this.allActiveElementsById[elementId];
			if (!htmlElement) {
				throw `Element id ${elementId} not found.`;
			}

			for (const name in properties) {
				if (properties.hasOwnProperty(name)) {
					(htmlElement as any)[name] = properties[name];
				}
			}

			return "ok";
		}

		/**
			* Get a property for an element.
			*/
		public getProperty(elementId: string, name: string): any {
			const htmlElement: HTMLElement | SVGElement = this.allActiveElementsById[elementId];
			if (!htmlElement) {
				throw `Element id ${elementId} not found.`;
			}

			return (htmlElement as any)[name] || "";
		}

		/**
			* Set the CSS style of a html element.
			*
			* To remove a value, set it to empty string.
			* @param styles A dictionary of styles to apply on html element.
			*/
		public setStyle(elementId: string, styles: { [name: string]: string }, setAsArranged: boolean = false): string {
			const htmlElement: HTMLElement | SVGElement = this.allActiveElementsById[elementId];
			if (!htmlElement) {
				throw `Element id ${elementId} not found.`;
			}

			for (const style in styles) {
				if (styles.hasOwnProperty(style)) {
					htmlElement.style.setProperty(style, styles[style]);
				}
			}

			if (setAsArranged) {
				htmlElement.classList.remove(WindowManager.unoUnarrangedClassName);
			}

			return "ok";
		}

		/**
			* Set the CSS style of a html element.
			*
			* To remove a value, set it to empty string.
			* @param styles A dictionary of styles to apply on html element.
			*/
		public resetStyle(elementId: string, names: string[]): string {
			const htmlElement: HTMLElement | SVGElement = this.allActiveElementsById[elementId];
			if (!htmlElement) {
				throw `Element id ${elementId} not found.`;
			}

			for(const name of names) {
				htmlElement.style.setProperty(name, "");
			}

			return "ok";
		}

		/**
			* Load the specified URL into a new tab or window
			* @param url URL to load
			* @returns "True" or "False", depending on whether a new window could be opened or not
			*/
		public open(url: string): string {
			const newWindow = window.open(url, "_blank");

			return newWindow != null
				? "True"
				: "False";
		}

		/**
			* Issue a browser alert to user
			* @param message message to display
			*/
		public alert(message: string): string {
			window.alert(message);

			return "ok";
        }

		/**
			* Add an event handler to a html element.
			*
			* @param eventName The name of the event
			* @param onCapturePhase true means "on trickle down", false means "on bubble up". Default is false.
			*/
		public registerEventOnView(
			elementId: string,
			eventName: string,
            onCapturePhase: boolean = false,
            eventFilter?: (event: Event) => boolean,
            eventExtractor?: (event: Event) => any): string {
			const htmlElement: HTMLElement | SVGElement = this.allActiveElementsById[elementId];
			if (!htmlElement) {
				throw `Element id ${elementId} not found.`;
			}

            const eventHandler = (event: Event) => {
                if (eventFilter && !eventFilter(event)) {
                    return;
                }

				const eventPayload =
					eventExtractor
						? `${eventExtractor(event)}`
                        : "";

				var handled = this.dispatchEvent(htmlElement, eventName, eventPayload);
				if (handled) {
					event.stopPropagation();
					if (event instanceof KeyboardEvent) {
						event.preventDefault();
					}
				}
			};

			htmlElement.addEventListener(eventName, eventHandler, onCapturePhase);

			return "ok";
		}

		/**
			* Set or replace the root content element.
			*/
		public setRootContent(elementId?: string): string {
			if (this.rootContent && this.rootContent.id === elementId) {
				return null; // nothing to do
			}

			if (this.rootContent) {
				// Remove existing
				this.containerElement.removeChild(this.rootContent);

				this.dispatchEvent(this.rootContent, "unloaded");
				this.rootContent.classList.remove(WindowManager.unoRootClassName);
			}

			if (!elementId) {
				return null;
			}

			// set new root
			const newRootElement = this.allActiveElementsById[elementId] as HTMLElement;
			newRootElement.classList.add(WindowManager.unoRootClassName);

			this.rootContent = newRootElement;

			this.dispatchEvent(this.rootContent, "loading");

			this.containerElement.appendChild(this.rootContent);

			this.dispatchEvent(this.rootContent, "loaded");
			newRootElement.classList.remove(WindowManager.unoUnarrangedClassName); // patch because root is not measured/arranged

			this.resize();

			return "ok";
		}

		/**
			* Set a view as a child of another one.
			*
			* "Loading" & "Loaded" events will be raised if nescessary.
			*
			* @param index Position in children list. Appended at end if not specified.
			*/
		public addView(parentId: string, childId: string, index?: number): string {
			const parentElement: HTMLElement | SVGElement = this.allActiveElementsById[parentId];
			if (!parentElement) {
				throw `addView: Parent element id ${parentId} not found.`;
			}
			const childElement: HTMLElement | SVGElement = this.allActiveElementsById[childId];
			if (!childElement) {
				throw `addView: Child element id ${parentId} not found.`;
			}

			const alreadyLoaded = this.GetIsConnectedToRootElement(childElement);
			const isLoading = !alreadyLoaded && this.GetIsConnectedToRootElement(parentElement);

			if (isLoading) {
				this.dispatchEvent(childElement, "loading");
			}

			if (index && index < parentElement.childElementCount) {
				const insertBeforeElement = parentElement.children[index];
				parentElement.insertBefore(childElement, insertBeforeElement);

			} else {
				parentElement.appendChild(childElement);
			}

			if (isLoading) {
				this.dispatchEvent(childElement, "loaded");
			}

			return "ok";
		}

		/**
			* Remove a child from a parent element.
			*
			* "Unloading" & "Unloaded" events will be raised if nescessary.
			*/
		public removeView(parentId: string, childId: string): string {
			const parentElement: HTMLElement | SVGElement = this.allActiveElementsById[parentId];
			if (!parentElement) {
				throw `removeView: Parent element id ${parentId} not found.`;
			}
			const childElement: HTMLElement | SVGElement = this.allActiveElementsById[childId];
			if (!childElement) {
				throw `removeView: Child element id ${parentId} not found.`;
			}

			const loaded = this.GetIsConnectedToRootElement(childElement);

			parentElement.removeChild(childElement);

			if (loaded) {
				this.dispatchEvent(childElement, "unloaded");
			}

			return "ok";
		}

		/**
			* Destroy a html element.
			*
			* The element won't be available anymore. Usually indicate the managed
			* version has been scavenged by the GC.
			*/
		public destroyView(viewId: string): string {
			const element: HTMLElement | SVGElement = this.allActiveElementsById[viewId];
			if (!element) {
				throw `destroyView: Element id ${viewId} not found.`;
			}

			if (element.parentElement) {
				element.parentElement.removeChild(element);
				delete this.allActiveElementsById[viewId];
			}

			return "ok";
        }

		public getBoundingClientRect(elementId: string): string {
			const htmlElement: HTMLElement | SVGElement = this.allActiveElementsById[elementId];
			if (!htmlElement) {
				throw `Element id ${elementId} not found.`;
			}

			var bounds = (<any>htmlElement).getBoundingClientRect();
			return `${bounds.left};${bounds.top};${bounds.right-bounds.left};${bounds.bottom-bounds.top}`;
		}

		public getBBox(elementId: string): string {
			const htmlElement: HTMLElement | SVGElement = this.allActiveElementsById[elementId];
			if (!htmlElement) {
				throw `Element id ${elementId} not found.`;
			}

			var bbox = (<any>htmlElement).getBBox();
			return `${bbox.x};${bbox.y};${bbox.width};${bbox.height}`;
		}

		/**
			* Use the Html engine to measure the element using specified constraints.
			*
			* @param maxWidth string containing width in pixels. Empty string means infinite.
			* @param maxHeight string containing height in pixels. Empty string means infinite.
			*/
		public measureView(viewId: string, maxWidth: string, maxHeight: string): string {
			const element = this.allActiveElementsById[viewId] as HTMLElement;
			if (!element) {
				throw `measureView: Element id ${viewId} not found.`;
			}

			const previousWidth = element.style.width;
            const previousHeight = element.style.height;
            const previousPosition = element.style.position;

			try {
				element.style.width = "";
                element.style.height = "";

                // This is required for an unconstrained measure (otherwise the parents size is taken into accound)
                element.style.position = "fixed";

				element.style.maxWidth = maxWidth ? `${maxWidth}px` : "";
				element.style.maxHeight = maxHeight ? `${maxHeight}px` : "";

                if (element.tagName.toUpperCase() == "IMG") {
                    const imgElement = element as HTMLImageElement;
                    const size = `${imgElement.naturalWidth};${imgElement.naturalHeight}`;
                    return size;
                }
                else {
                    const resultWidth = element.offsetWidth ? element.offsetWidth : element.clientWidth;
                    const resultHeight = element.offsetHeight ? element.offsetHeight : element.clientHeight;
                    const size = `${resultWidth};${resultHeight}`;

                    return size;
                }
			} finally {
				element.style.width = previousWidth;
                element.style.height = previousHeight;
                element.style.position = previousPosition;

				element.style.maxWidth = "";
				element.style.maxHeight = "";
			}
		}

		public setPointerCapture(viewId: string, pointerId: number): string {
			const element = this.allActiveElementsById[viewId] as HTMLElement;
			if (!element) {
				throw `setPointerCapture: Element id ${viewId} not found.`;
			}

			element.setPointerCapture(pointerId);

			return "ok";
		}

		public releasePointerCapture(viewId: string, pointerId: number): string {
			const element = this.allActiveElementsById[viewId] as HTMLElement;
			if (!element) {
				throw `releasePointerCapture: Element id ${viewId} not found.`;
			}

			element.releasePointerCapture(pointerId);

			return "ok";
		}

		public focusView(elementId: string): string {
			const htmlElement: HTMLElement | SVGElement = this.allActiveElementsById[elementId];
			if (!htmlElement) {
				throw `Element id ${elementId} not found.`;
			}

			if (!(htmlElement instanceof HTMLElement)) {
				throw `Element id ${elementId} is not focusable.`;
			}

			htmlElement.focus();

			return "ok";
		}

		/**
			* Set the Html content for an element.
			*
			* Those html elements won't be available as XamlElement in managed code.
			* WARNING: you should avoid mixing this and `addView` for the same element.
			*/
		public setHtmlContent(viewId: string, html: string): string {
			const element: HTMLElement | SVGElement = this.allActiveElementsById[viewId];
			if (!element) {
				throw `setHtmlContent: Element id ${viewId} not found.`;
			}

			element.innerHTML = html;

			return "ok";
		}

		/**
			* Remove the loading indicator.
			*
			* In a future version it will also handle the splashscreen.
			*/
		public activate(): string {
			this.removeLoading();
			return "ok";
		}

		private init() {
		}

		private static initMethods() {
			if (!WindowManager.assembly) {
				WindowManager.assembly = MonoRuntime.assembly_load("Uno.UI");

				if (!WindowManager.assembly) {
					throw `Unable to find assembly Uno.UI`;
				}
			}

			if (!WindowManager.resizeMethod) {
				const type = MonoRuntime.find_class(WindowManager.assembly, "Windows.UI.Xaml", "Window");

				if (!type) {
					throw `Unable to find type Windows.UI.Xaml.Window`;
				}

				WindowManager.resizeMethod = MonoRuntime.find_method(type, "Resize", -1);

				if (!WindowManager.resizeMethod) {
					throw `Unable to find Windows.UI.Xaml.Window.Resize method`;
				}
			}

			if (!WindowManager.dispatchEventMethod) {
				const type = MonoRuntime.find_class(WindowManager.assembly, "Windows.UI.Xaml", "UIElement");
				WindowManager.dispatchEventMethod = MonoRuntime.find_method(type, "DispatchEvent", -1);

				if (!WindowManager.dispatchEventMethod) {
					throw `Unable to find Windows.UI.Xaml.UIElement.DispatchEvent method`;
				}
			}
		}

		private initDom() {
			this.containerElement = (document.getElementById(this.containerElementId) as HTMLDivElement);
			if (!this.containerElement) {
				// If not found, we simply create a new one.
				this.containerElement = document.createElement("div");
				document.body.appendChild(this.containerElement);
			}

			window.addEventListener("resize", x => this.resize());
		}

		private removeLoading() {

			if (!this.loadingElementId) {
				return;
			}

			const element = document.getElementById(this.loadingElementId);
			if (element) {
				element.parentElement.removeChild(element);
            }

            // UWP Window's default background is white.
            const body = document.getElementsByTagName("body")[0];
            body.style.backgroundColor = '#fff';
		}

		private resize() {
			const sizeStr = this.getMonoString(`${window.innerWidth};${window.innerHeight}`);
			MonoRuntime.call_method(WindowManager.resizeMethod, null, [sizeStr]);
		}

		private dispatchEvent(element: HTMLElement | SVGElement, eventName: string, eventPayload: string = null): boolean {
			const htmlId = element.getAttribute("XamlHandle");
			
			// console.debug(`${element.getAttribute("id")}: Raising event ${eventName}.`);

			if (!htmlId) {
				throw `No attribute XamlHandle on element ${element}. Can't raise event.`;
			}

            const htmlIdStr = this.getMonoString(htmlId);
            const eventNameStr = this.getMonoString(eventName);
            const eventPayloadStr = this.getMonoString(eventPayload);

			var handledHandle = MonoRuntime.call_method(WindowManager.dispatchEventMethod, null, [htmlIdStr, eventNameStr, eventPayloadStr]);
            var handledStr = this.fromMonoString(handledHandle);
			var handled = handledStr == "True";
			return handled;
        }

        private getMonoString(str: string): Interop.IMonoStringHandle {
            return str ? MonoRuntime.mono_string(str) : null;
        }

        private fromMonoString(strHandle: Interop.IMonoStringHandle): string {
            return strHandle ? MonoRuntime.conv_string(strHandle) : "";
        }

		private GetIsConnectedToRootElement(element: HTMLElement | SVGElement): boolean {
			const rootElement = this.rootContent;

			if (!rootElement) {
				return false;
			}
			return rootElement === element || rootElement.contains(element);
		}
	}

	document.addEventListener("DOMContentLoaded", () => WindowManager.setupSplashScreen());
}
